// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// $Id: CoordGmsImpl.java,v 1.13 2004/09/08 09:17:17 belaban Exp $
using System;
using System.Collections;
using Alachisoft.NCache.Common.Threading;
using Alachisoft.NCache.Common.Net;
using Alachisoft.NCache.Common.Enum;

namespace Alachisoft.NGroups.Protocols.pbcast
{
    /// <summary> Coordinator role of the Group MemberShip (GMS) protocol. Accepts JOIN and LEAVE requests and emits view changes
    /// accordingly.
    /// </summary>
    /// <author>  Bela Ban
    /// </author>
    internal class CoordGmsImpl : GmsImpl
    {
        private void InitBlock()
        {
            merge_task = new MergeTask(this);
        }
        internal bool merging = false;
        internal MergeTask merge_task;
        internal System.Collections.ArrayList merge_rsps = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(11));
        // for MERGE_REQ/MERGE_RSP correlation, contains MergeData elements
        internal object merge_id = null;
        private object connection_break_mutex = new object(); //synchronizes the thread informing the connection breakage.

        internal ArrayList initial_mbrs = ArrayList.Synchronized(new ArrayList(11));
        internal bool initial_mbrs_received = false;
        internal Promise join_promise = new Promise();

        internal Promise connectedNodesPromise = new Promise();
        internal Hashtable connectedNodesMap = Hashtable.Synchronized(new Hashtable());
        internal int missingResults;
        internal int conNodesReqId = 0;
        ArrayList viewRejectingMembers = new ArrayList();
        private const int MAX_CLUSTER_MBRS = 2;

        public CoordGmsImpl(GMS g)
        {
            InitBlock();

            if (String.IsNullOrEmpty(g.UniqueID))
            {
                _uniqueId = Guid.NewGuid().ToString();
                g.UniqueID = _uniqueId;
            }
            else
            {
                _uniqueId = g.UniqueID;
            }

            gms = g;
        }

        public override void join(Address mbr)
        {
            wrongMethod("join");
        }

        /// <summary>The coordinator itself wants to leave the group </summary>
        public override void leave(Address mbr)
        {
            if (mbr == null)
            {
                gms.Stack.NCacheLog.Error("CoordGmsImpl.leave", "member's address is null !");
                return;
            }
            if (mbr.Equals(gms.local_addr))
                leaving = true;
            handleLeave(mbr, false); // regular leave
        }

        public override void handleJoinResponse(JoinRsp join_rsp)
        {
            join_promise.SetResult(join_rsp); // will wake up join() method
            gms.Stack.NCacheLog.CriticalInfo("CoordGMSImpl.handleJoin called at startup");
            wrongMethod("handleJoinResponse");
        }

        public override void handleLeaveResponse()
        {
            ; // safely ignore this
        }

        public override void suspect(Address mbr)
        {
            handleSuspect(mbr);
        }

        public override void unsuspect(Address mbr)
        {

        }

        /// <summary> Invoked upon receiving a MERGE event from the MERGE layer. Starts the merge protocol.
        /// See description of protocol in DESIGN.
        /// </summary>
        /// <param name="other_coords">A list of coordinators (including myself) found by MERGE protocol
        /// </param>
        public override void merge(System.Collections.ArrayList other_coords)
        {
            Membership tmp;
            Address leader = null;

            if (merging)
            {
                gms.Stack.NCacheLog.Warn("CoordGmsImpl.merge", "merge already in progress, discarded MERGE event");
                return;
            }

            if (other_coords == null)
            {
                gms.Stack.NCacheLog.Warn("CoordGmsImpl.merge", "list of other coordinators is null. Will not start merge.");
                return;
            }

            if (other_coords.Count <= 1)
            {
                gms.Stack.NCacheLog.Error("CoordGmsImpl.merge", "number of coordinators found is " + other_coords.Count + "; will not perform merge");
                return;
            }

            /* Establish deterministic order, so that coords can elect leader */
            tmp = new Membership(other_coords);
            tmp.sort();
            leader = (Address)tmp.elementAt(0);
            gms.Stack.NCacheLog.Debug("coordinators in merge protocol are: " + tmp);
            if (leader.Equals(gms.local_addr))
            {
                gms.Stack.NCacheLog.Debug("I (" + leader + ") will be the leader. Starting the merge task");
                startMergeTask(other_coords);
            }
        }

        /// <summary> Get the view and digest and send back both (MergeData) in the form of a MERGE_RSP to the sender.
        /// If a merge is already in progress, send back a MergeData with the merge_rejected field set to true.
        /// </summary>
        public override void handleMergeRequest(Address sender, object merge_id)
        {
            Digest digest;
            View view;

            if (sender == null)
            {
                gms.Stack.NCacheLog.Error("CoordGmsImpl.handleMergeRequest", "sender == null; cannot send back a response");
                return;
            }
            if (merging)
            {
                gms.Stack.NCacheLog.Error("CoordGmsImpl.handleMergeRequest", "merge already in progress");
                sendMergeRejectedResponse(sender);
                return;
            }
            merging = true;
            this.merge_id = merge_id;

            gms.Stack.NCacheLog.Debug("CoordGmsImpl.handleMergeRequest", "sender=" + sender + ", merge_id=" + merge_id);

            digest = gms.Digest;
            view = new View(gms.view_id.Copy(), gms.members.Members);
            view.CoordinatorGmsId = gms.unique_id;
            sendMergeResponse(sender, view, digest);
        }


        internal virtual MergeData getMergeResponse(Address sender, object merge_id)
        {
            Digest digest;
            View view;
            MergeData retval;

            if (sender == null)
            {
                gms.Stack.NCacheLog.Error("CoordGmsImpl.getMergeResponse", "sender == null; cannot send back a response");
                return null;
            }
            if (merging)
            {
                gms.Stack.NCacheLog.Error("CoordGmsImpl.getMergeResponse", "merge already in progress");
                retval = new MergeData(sender, null, null);
                retval.merge_rejected = true;
                return retval;
            }
            merging = true;
            this.merge_id = merge_id;

            gms.Stack.NCacheLog.Debug("sender=" + sender + ", merge_id=" + merge_id);

            digest = gms.Digest;
            view = new View(gms.view_id.Copy(), gms.members.Members);
            retval = new MergeData(sender, view, digest);
            retval.view = view;
            retval.digest = digest;
            return retval;
        }


        public override void handleMergeResponse(MergeData data, object merge_id)
        {
            if (data == null)
            {
                gms.Stack.NCacheLog.Error("CoordGmsImpl.handleMergeResponse", "merge data is null");
                return;
            }
            if (merge_id == null || this.merge_id == null)
            {

                gms.Stack.NCacheLog.Error("CoordGmsImpl.handleMergeResponse", "merge_id (" + merge_id + ") or this.merge_id (" + this.merge_id + ") == null (sender=" + data.Sender + ").");
                return;
            }

            if (!this.merge_id.Equals(merge_id))
            {
                gms.Stack.NCacheLog.Error("CoordGmsImpl.handleMergeResponse", "this.merge_id (" + this.merge_id + ") is different from merge_id (" + merge_id + ')');
                return;
            }

            lock (merge_rsps.SyncRoot)
            {
                if (!merge_rsps.Contains(data))
                {
                    merge_rsps.Add(data);
                    System.Threading.Monitor.PulseAll(merge_rsps.SyncRoot);
                }
            }
        }

        /// <summary> If merge_id != this.merge_id --> discard
        /// Else cast the view/digest to all members of this group.
        /// </summary>
        public override void handleMergeView(MergeData data, object merge_id)
        {
            if (merge_id == null || this.merge_id == null || !this.merge_id.Equals(merge_id))
            {
                gms.Stack.NCacheLog.Error("CoordGmsImpl.handleMergeView", "merge_ids don't match (or are null); merge view discarded");
                return;
            }
            gms.castViewChange(data.view, data.digest);
            merging = false;
            merge_id = null;
        }

        public override void handleMergeCancelled(object merge_id)
        {
            if (merge_id != null && this.merge_id != null && this.merge_id.Equals(merge_id))
            {

                gms.Stack.NCacheLog.Debug("merge was cancelled (merge_id=" + merge_id + ')');
                this.merge_id = null;
                merging = false;
            }
        }

        /// <summary> Computes the new view (including the newly joined member) and get the digest from PBCAST.
        /// Returns both in the form of a JoinRsp
        /// </summary>
        public override JoinRsp handleJoin(Address mbr, string subGroup_name, string gmsId, ref bool acquireHashmap)
        {
            lock (this)
            {
                System.Collections.ArrayList new_mbrs = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(1));
                View v = null;
                Digest d, tmp;


                gms.Stack.NCacheLog.CriticalInfo("CoordGmsImpl.HandleJoin", "Member: " + mbr + " is joining the cluster.");

                if (gms.local_addr.Equals(mbr))
                {
                    gms.Stack.NCacheLog.Error("CoordGmsImpl.handleJoin", "cannot join myself !");
                    return null;
                }

                if (gms.members.contains(mbr))
                {
                    gms.Stack.NCacheLog.Error("CoordGmsImpl.handleJoin()", "member " + mbr + " already present; returning existing view " + Global.CollectionToString(gms.members.Members));
                    acquireHashmap = false;
                    View view = new View(gms.view_id, gms.members.Members);
                    view.CoordinatorGmsId = gms.unique_id;
                    JoinRsp rsp = new JoinRsp(view, gms.Digest);
                    rsp.View.SequencerTbl = gms._subGroupMbrsMap;
                    rsp.View.MbrsSubgroupMap = gms._mbrSubGroupMap;
                    return rsp;
                    // already joined: return current digest and membership
                }
                new_mbrs.Add(mbr);
                //=====================================
                // update the subGroupMbrsMap and mbrSubGroupMap
                if (gms._subGroupMbrsMap.Contains(subGroup_name))
                {
                    lock (gms._subGroupMbrsMap.SyncRoot)
                    {
                        System.Collections.ArrayList groupMbrs = (System.Collections.ArrayList)gms._subGroupMbrsMap[subGroup_name];
                        if (!groupMbrs.Contains(mbr))
                            groupMbrs.Add(mbr);
                    }
                }
                else
                {
                    lock (gms._subGroupMbrsMap.SyncRoot)
                    {
                        System.Collections.ArrayList groupMbrs = new System.Collections.ArrayList();
                        groupMbrs.Add(mbr);
                        gms._subGroupMbrsMap[subGroup_name] = groupMbrs;
                    }
                }

                if (!gms._mbrSubGroupMap.Contains(mbr))
                {
                    lock (gms._mbrSubGroupMap.SyncRoot)
                    {
                        gms._mbrSubGroupMap[mbr] = subGroup_name;
                    }
                }
                //=====================================
                tmp = gms.Digest; // get existing digest
                if (tmp == null)
                {
                    gms.Stack.NCacheLog.Error("CoordGmsImpl.handleJoin", "received null digest from GET_DIGEST: will cause JOIN to fail");
                    return null;
                }

                gms.Stack.NCacheLog.Debug("got digest=" + tmp);

                d = new Digest(tmp.size() + 1);
                // create a new digest, which contains 1 more member
                d.add(tmp); // add the existing digest to the new one
                d.add(mbr, 0, 0);
                // ... and add the new member. it's first seqno will be 1
                v = gms.getNextView(new_mbrs, null, null);
                v.SequencerTbl = gms._subGroupMbrsMap;
                v.MbrsSubgroupMap = gms._mbrSubGroupMap;
                v.AddGmsId(mbr, gmsId);

                //add coordinator own's gms id[bug fix]; so that new member could know cordinator id
                v.AddGmsId(gms.local_addr, gms.unique_id);

                if (gms.GmsIds != null)
                {
                    Hashtable gmsIds = gms.GmsIds.Clone() as Hashtable;
                    IDictionaryEnumerator ide = gmsIds.GetEnumerator();
                    while (ide.MoveNext())
                    {
                        v.AddGmsId((Address)ide.Key,(string) ide.Value);
                    }
                }

                gms.Stack.NCacheLog.Debug("joined member " + mbr + ", view is " + v);

                return new JoinRsp(v, d);
            }
        }

        /// <summary>Exclude <code>mbr</code> from the membership. If <code>suspected</code> is true, then
        /// this member crashed and therefore is forced to leave, otherwise it is leaving voluntarily.
        /// </summary>
        public override void handleLeave(Address mbr, bool suspected)
        {
            lock (this)
            {

                if (gms.disconnected_nodes.Contains(mbr))
                    gms.disconnected_nodes.Remove(mbr);

                Address sameNode = null;
                if (gms.isPartReplica)
                {
                    Membership mbrShip = gms.members.copy();

                    if (mbrShip.contains(mbr))
                    {
                        for (int i = 0; i < mbrShip.size(); i++)
                        {
                            Address other = mbrShip.elementAt(i);
                            if (other != null && !other.Equals(mbr))
                            {
                                if (other.IpAddress.Equals(mbr.IpAddress))
                                {
                                    sameNode = other;
                                    break;
                                }
                            }
                        }
                    }
                }
                ArrayList leavingNodes = new ArrayList();

                if (sameNode != null && !sameNode.IpAddress.Equals(gms.local_addr.IpAddress))
                {
                    if (sameNode.Port > mbr.Port)
                    {
                        leavingNodes.Add(sameNode);
                        leavingNodes.Add(mbr);
                    }
                    else
                    {
                        leavingNodes.Add(mbr);
                        leavingNodes.Add(sameNode);
                    }
                }
                else
                {
                    leavingNodes.Add(mbr);
                }
                System.Collections.ArrayList v = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(1));
                // contains either leaving mbrs or suspected mbrs
                gms.Stack.NCacheLog.Debug("pbcast.CoordGmsImpl.handleLeave()", "mbr=" + mbr);
                foreach (Address leavingNode in leavingNodes)
                {
                    if (!gms.members.contains(leavingNode))
                    {
                        gms.Stack.NCacheLog.Debug("pbcast.CoordGmsImpl.handleLeave()", "mbr " + leavingNode + " is not a member !");
                        if (!suspected) sendLeaveResponse(leavingNode); // send an ack to the leaving member
                        return;
                    }

                    if (gms.view_id == null)
                    {
                        // we're probably not the coord anymore (we just left ourselves), let someone else do it
                        // (client will retry when it doesn't get a response
                        gms.Stack.NCacheLog.Debug("pbcast.CoordGmsImpl.handleLeave()", "gms.view_id is null, I'm not the coordinator anymore (leaving=" + leaving + "); the new coordinator will handle the leave request");
                        return;
                    }

                    if (!suspected) sendLeaveResponse(leavingNode); // send an ack to the leaving member

                    //===============================================
                    //this mbr will now be removed from the list of live mbrs.
                    //so properly update the gms' subgroupMbrMap so that all the 
                    //nodes know who is the next sequencer for their subgroup.
                    string subGroup = (string)gms._mbrSubGroupMap[leavingNode];
                    if (subGroup != null)
                    {
                        lock (gms._mbrSubGroupMap.SyncRoot)
                        {
                            gms._mbrSubGroupMap.Remove(leavingNode);
                        }
                        lock (gms._subGroupMbrsMap.SyncRoot)
                        {
                            System.Collections.ArrayList subGroupMbrs = (System.Collections.ArrayList)gms._subGroupMbrsMap[subGroup];
                            if (subGroupMbrs != null)
                            {
                                subGroupMbrs.Remove(leavingNode);
                                if (subGroupMbrs.Count == 0)
                                {
                                    gms._subGroupMbrsMap.Remove(subGroup);
                                }
                            }
                        }
                    }
                    v.Add(leavingNode);
                    //requests the gms to acquire a new map after this member leaves.
                    System.Collections.ArrayList mbrs = new System.Collections.ArrayList(1);
                    mbrs.Add(leavingNode);
                    gms.acquireHashmap(mbrs, false, subGroup);
                }
                //===============================================


                if (suspected)
                    gms.castViewChange(null, null, v, gms._hashmap);
                else
                    gms.castViewChange(null, v, null, gms._hashmap);
            }
        }

        internal virtual void sendLeaveResponse(Address mbr)
        {
            Message msg = new Message(mbr, null, null);
            GmsHDR hdr = new GmsHDR(GmsHDR.LEAVE_RSP);
            msg.putHeader(HeaderType.GMS, hdr);
            gms.passDown(new Event(Event.MSG, msg));
        }

        /// <summary> Called by the GMS when a VIEW is received.</summary>
        /// <param name="new_view">The view to be installed
        /// </param>
        /// <param name="digest">  If view is a MergeView, digest contains the seqno digest of all members and has to
        /// be set by GMS
        /// </param>
        public override void handleViewChange(View new_view, Digest digest)
        {
            System.Collections.ArrayList mbrs = new_view.Members;
            if (digest != null)
            {
                gms.Stack.NCacheLog.Debug("view=" + new_view + ", digest=" + digest);
            }
            else
            {
                gms.Stack.NCacheLog.Debug("view=" + new_view);
            }

            if (leaving && !mbrs.Contains(gms.local_addr))
                return;
            gms.installView(new_view, digest);

            lock (viewRejectingMembers.SyncRoot)
            {
                //we handle the request of those nodes who have rejected our this view
                Address rejectingMbr;
                for (int i = 0; i < viewRejectingMembers.Count; i++)
                {
                    rejectingMbr = viewRejectingMembers[i] as Address;
                    handleViewRejected(rejectingMbr);
                }
                viewRejectingMembers.Clear();
            }
        }

        public override void handleSuspect(Address mbr)
        {
            if (mbr.Equals(gms.local_addr))
            {
                gms.Stack.NCacheLog.Warn("I am the coord and I'm being am suspected -- will probably leave shortly");
                return;
            }
            handleLeave(mbr, true); // irregular leave - forced
        }
        public override void handleNodeRejoining(Address node)
        {
            handleInformNodeRejoining(gms.local_addr, node);
        }
        public override void handleCanNotConnectTo(Address src, System.Collections.ArrayList failedNodes)
        {
            if (src != null && failedNodes != null)
            {
                Hashtable nodeGmsIds = new Hashtable();
                string sourceGmsId = gms.GetNodeGMSId(src);
                foreach (Address node in failedNodes)
                {
                    string gmsId = gms.GetNodeGMSId(node);
                    if (gmsId != null) nodeGmsIds[node] = gmsId;
                }
                if (gms.Stack.NCacheLog.IsInfoEnabled) gms.Stack.NCacheLog.Info("CoodGmsImpl.handleCanNotConnectTo", src + " can not connect to " + Global.CollectionToString(failedNodes));
                foreach (Address node in failedNodes)
                {
                    
                   
                    if (!gms.VerifySuspect(node))
                    {
                        Membership mbrs = gms.members.copy();
                        Address seniorNode = mbrs.DetermineSeniority(src, node);

                        if (gms.Stack.NCacheLog.IsInfoEnabled) gms.Stack.NCacheLog.Info("CoodGmsImpl.handleCanNotConnectTo", "senior node " + seniorNode);
                        string nodeGmsId = nodeGmsIds[node] as string;
                        
                        if (seniorNode.Equals(src))
                        {
                            //suspect has to leave the cluster
                            AskToLeaveCluster(node, nodeGmsId);
                        }
                        else
                        {
                            //informer has to leave the cluster
                            AskToLeaveCluster(src, sourceGmsId);
                        }

                    }
                    else
                        if (gms.Stack.NCacheLog.IsInfoEnabled) gms.Stack.NCacheLog.Info("CoodGmsImpl.handleCanNotConnectTo", node + " is already declared dead");

                }
            }
        }
        public override void handleInformNodeRejoining(Address sender, Address node)
        {
            if (node != null)
            {
                if (gms.Stack.NCacheLog.IsInfoEnabled) gms.Stack.NCacheLog.Info("CoordinatorGmsImpl.handleInformNodeRejoining", sender.ToString() + " informed about rejoining with " + node);
                if (gms.members.contains(node))
                {
                    ViewId viewId = gms.GetNextViewId();
                    GmsHDR header = new GmsHDR(GmsHDR.RESET_ON_NODE_REJOINING, node);
                    header.view = new View(viewId, gms.members.Clone() as ArrayList);
                    header.view.CoordinatorGmsId = gms.unique_id;
                    Message rejoiningMsg = new Message(null, null, new byte[0]);
                    rejoiningMsg.putHeader(HeaderType.GMS, header);
                    gms.passDown(new Event(Event.MSG, rejoiningMsg, Priority.High));
                }
            }
        }
        public override void handleResetOnNodeRejoining(Address sender, Address node,View view)
        {
            gms.handleResetOnNodeRejoining(sender, node, view);
        }
        public override void handleConnectionBroken(Address informer, Address suspected)
        {
            if (gms.Stack.NCacheLog.IsInfoEnabled) gms.Stack.NCacheLog.Info("CoodGmsImpl.handleConnectionBroken", informer + " informed about connection breakage with " + suspected);

            ///synchronizes the multiple parallel requests so that one request is executed
            ///at a time.

            string nodeGmsId = gms.GetNodeGMSId(suspected);
            string sourceGmsId = gms.GetNodeGMSId(informer);

            lock (connection_break_mutex)
            {
                if (!gms.VerifySuspect(suspected))
                {
                    if (gms.Stack.NCacheLog.IsInfoEnabled) gms.Stack.NCacheLog.Info("CoodGmsImpl.handleConnectionBroken", suspected + " is not dead");

                    //Do not delete this code, we may use it in future.
                    //Node is not dead. it means that there is some trouble on the network
                    //let's solve the puzzle. We get the connection mesh from all the nodes.
                    //connectedNodesPromise.Reset();
                    //lock (connectedNodesMap.SyncRoot)
                    //{
                    //    conNodesReqId++;
                    //    connectedNodesMap.Clear();
                    //    ArrayList currentMbrs = gms.members.Members;
                    //    currentMbrs.Remove(gms.local_addr); //exclude ourself
                    //    foreach (Address mbr in currentMbrs)
                    //    {
                    //        connectedNodesMap.Add(mbr, null);
                    //    }
                    //    missingResults = currentMbrs.Count; 
                    //}
                    ////Approximate time for waiting = 1000 (ms) * no of nodes
                    //long timeToWait = 1000 * connectedNodesMap.Count; 

                    //Message msg = new Message(null, null, new byte[0]);
                    //msg.putHeader(HeaderType.GMS, new GMS.HDR(GMS.HDR.CONNECTED_NODES_REQUEST,conNodesReqId));
                    //gms.passDown(new Event(Event.MSG, msg, Priority.Critical));

                    ////wait for the response from all the nodes.
                    //connectedNodesPromise.WaitResult(timeToWait);

                    Membership mbrs = gms.members.copy();
                    Address seniorNode = mbrs.DetermineSeniority(informer, suspected);

                    if (gms.Stack.NCacheLog.IsInfoEnabled) gms.Stack.NCacheLog.Info("CoodGmsImpl.handleConnectionBroken", "senior node " + seniorNode);

                    if (seniorNode.Equals(informer))
                    {
                        //suspect has to leave the cluster
                        AskToLeaveCluster(suspected,nodeGmsId);
                    }
                    else
                    {
                        //informer has to leave the cluster
                        AskToLeaveCluster(informer,sourceGmsId);
                    }

                }
            }
        }

        /// <summary>
        /// When we broadcast a view and any of the member reject the view then
        /// we should also remove it from the our membership list.
        /// </summary>
        /// <param name="mbrRejected"></param>
        public override void handleViewRejected(Address mbrRejected)
        {
            lock (viewRejectingMembers.SyncRoot)
            {
                if (gms.determineCoordinator().Equals(gms.local_addr))
                {
                    if (gms.Stack.NCacheLog.IsInfoEnabled) gms.Stack.NCacheLog.Info("CoodGmsImpl.handleViewRejected", mbrRejected + " rejcted the view");

                    handleLeave(mbrRejected, false);
                }
                else
                {
                    if (gms.Stack.NCacheLog.IsInfoEnabled) gms.Stack.NCacheLog.Info("CoodGmsImpl.handleViewRejected", mbrRejected + " rejcted the view, but we have not installed the view ourself");

                    //It is the case when we have broadcasted the view to all the members
                    //including ourself. A member rejection is received before we have
                    //not installed this view yet.

                    viewRejectingMembers.Add(mbrRejected);

                }
            }
        }
        public override void stop()
        {
            leaving = true;
            merge_task.stop();
        }

        public void AskToLeaveCluster(Address leavingMember, string urGmsId)
        {
            if (gms.Stack.NCacheLog.IsInfoEnabled) gms.Stack.NCacheLog.Info("CoodGmsImpl.AskToLeaveCluster", leavingMember + " is requested to leave the cluster");

            Message msg = new Message(leavingMember, null, new byte[0]);
            GmsHDR  hdr =  new GmsHDR(GmsHDR.LEAVE_CLUSTER, gms.local_addr);
            hdr.arg = urGmsId;
            msg.putHeader(HeaderType.GMS,hdr);
            gms.passDown(new Event(Event.MSG, msg, Priority.High)); ;
        }

        /* ------------------------------------------ Private methods ----------------------------------------- */

        internal virtual void startMergeTask(System.Collections.ArrayList coords)
        {
            merge_task.start(coords);
        }

        internal virtual void stopMergeTask()
        {
            merge_task.stop();
        }

        /// <summary> Sends a MERGE_REQ to all coords and populates a list of MergeData (in merge_rsps). Returns after coords.size()
        /// response have been received, or timeout msecs have elapsed (whichever is first).<p>
        /// If a subgroup coordinator rejects the MERGE_REQ (e.g. because of participation in a different merge),
        /// <em>that member will be removed from coords !</em>
        /// </summary>
        /// <param name="coords">A list of Addresses of subgroup coordinators (inluding myself)
        /// </param>
        /// <param name="timeout">Max number of msecs to wait for the merge responses from the subgroup coords
        /// </param>
        internal virtual void getMergeDataFromSubgroupCoordinators(System.Collections.ArrayList coords, long timeout)
        {
            Message msg;
            GmsHDR hdr;
            Address coord;
            long curr_time, time_to_wait = 0, end_time;
            int num_rsps_expected = 0;

            if (coords == null || coords.Count <= 1)
            {
                gms.Stack.NCacheLog.Error("CoordGmsImpl.getMergeDataFromSubgroupCoordinator", "coords == null or size <= 1");
                return;
            }

            lock (merge_rsps.SyncRoot)
            {
                merge_rsps.Clear();

                gms.Stack.NCacheLog.Debug("sending MERGE_REQ to " + Global.CollectionToString(coords));
                for (int i = 0; i < coords.Count; i++)
                {
                    coord = (Address)coords[i];

                    if (gms.local_addr != null && gms.local_addr.Equals(coord))
                    {
                        merge_rsps.Add(getMergeResponse(gms.local_addr, merge_id));
                        continue;
                    }

                    msg = new Message(coord, null, null);
                    hdr = new GmsHDR(GmsHDR.MERGE_REQ);
                    hdr.mbr = gms.local_addr;
                    hdr.merge_id = merge_id;
                    msg.putHeader(HeaderType.GMS, hdr);
                    gms.passDown(new Event(Event.MSG, msg));
                }

                // wait until num_rsps_expected >= num_rsps or timeout elapsed
                num_rsps_expected = coords.Count;
                curr_time = (System.DateTime.Now.Ticks - 621355968000000000) / 10000;
                end_time = curr_time + timeout;
                while (end_time > curr_time)
                {
                    time_to_wait = end_time - curr_time;

                    gms.Stack.NCacheLog.Debug("waiting for " + time_to_wait + " msecs for merge responses");
                    if (merge_rsps.Count < num_rsps_expected)
                    {
                        try
                        {
                            System.Threading.Monitor.Wait(merge_rsps.SyncRoot, TimeSpan.FromMilliseconds(time_to_wait));
                        }
                        catch (System.Exception ex)
                        {
                            gms.Stack.NCacheLog.Error("CoordGmsImpl.getMergeDataFromSubgroupCoordinators()", ex.ToString());
                        }
                    }
                    
                    if (time_to_wait < 0)
                    {
                        gms.Stack.NCacheLog.Fatal("[Timeout]CoordGmsImpl.getMergeDataFromSubgroupCoordinators:" + time_to_wait);
                    }

                    gms.Stack.NCacheLog.Debug("num_rsps_expected=" + num_rsps_expected + ", actual responses=" + merge_rsps.Count);

                    if (merge_rsps.Count >= num_rsps_expected)
                        break;
                    curr_time = (System.DateTime.Now.Ticks - 621355968000000000) / 10000;
                }
            }
        }

        /// <summary> Generates a unique merge id by taking the local address and the current time</summary>
        internal object generateMergeId()
        {
            return new ViewId(gms.local_addr, (System.DateTime.Now.Ticks - 621355968000000000) / 10000);
            // we're (ab)using ViewId as a merge id
        }

        /// <summary> Merge all MergeData. All MergeData elements should be disjunct (both views and digests). However,
        /// this method is prepared to resolve duplicate entries (for the same member). Resolution strategy for
        /// views is to merge only 1 of the duplicate members. Resolution strategy for digests is to take the higher
        /// seqnos for duplicate digests.<p>
        /// After merging all members into a Membership and subsequent sorting, the first member of the sorted membership
        /// will be the new coordinator.
        /// </summary>
        /// <param name="v">A list of MergeData items. Elements with merge_rejected=true were removed before. Is guaranteed
        /// not to be null and to contain at least 1 member.
        /// </param>
        internal virtual MergeData consolidateMergeData(System.Collections.ArrayList v)
        {
            MergeData ret = null;
            MergeData tmp_data;
            long logical_time = 0; // for new_vid
            ViewId new_vid, tmp_vid;
            MergeView new_view;
            View tmp_view;
            Membership new_mbrs = new Membership();
            int num_mbrs = 0;
            Digest new_digest = null;
            Address new_coord;
            System.Collections.ArrayList subgroups = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(11));
            // contains a list of Views, each View is a subgroup

            for (int i = 0; i < v.Count; i++)
            {
                tmp_data = (MergeData)v[i];

                gms.Stack.NCacheLog.Debug("merge data is " + tmp_data);

                tmp_view = tmp_data.View;
                if (tmp_view != null)
                {
                    tmp_vid = tmp_view.Vid;
                    if (tmp_vid != null)
                    {
                        // compute the new view id (max of all vids +1)
                        logical_time = System.Math.Max(logical_time, tmp_vid.Id);
                    }
                }
                // merge all membership lists into one (prevent duplicates)
                new_mbrs.add(tmp_view.Members);
                subgroups.Add(tmp_view.Clone());
            }

            // the new coordinator is the first member of the consolidated & sorted membership list
            new_mbrs.sort();
            num_mbrs = new_mbrs.size();
            new_coord = num_mbrs > 0 ? (Address)new_mbrs.elementAt(0) : null;
            if (new_coord == null)
            {
                gms.Stack.NCacheLog.Error("CoordGmsImpl.consolodateMergeData", "new_coord is null.");
                return null;
            }
            // should be the highest view ID seen up to now plus 1
            new_vid = new ViewId(new_coord, logical_time + 1);

            // determine the new view
            new_view = new MergeView(new_vid, new_mbrs.Members, subgroups);

            gms.Stack.NCacheLog.Debug("new merged view will be " + new_view);

            // determine the new digest
            new_digest = consolidateDigests(v, num_mbrs);
            if (new_digest == null)
            {
                gms.Stack.NCacheLog.Error("CoordGmsImpl.consolidateMergeData", "digest could not be consolidated.");
                return null;
            }

            gms.Stack.NCacheLog.Debug("consolidated digest=" + new_digest);

            ret = new MergeData(gms.local_addr, new_view, new_digest);
            return ret;
        }

        /// <summary> Merge all digests into one. For each sender, the new value is min(low_seqno), max(high_seqno),
        /// max(high_seqno_seen)
        /// </summary>
        internal virtual Digest consolidateDigests(System.Collections.ArrayList v, int num_mbrs)
        {
            MergeData data;
            Digest tmp_digest, retval = new Digest(num_mbrs);

            for (int i = 0; i < v.Count; i++)
            {
                data = (MergeData)v[i];
                tmp_digest = data.Digest;
                if (tmp_digest == null)
                {
                    gms.Stack.NCacheLog.Error("tmp_digest == null; skipping");
                    continue;
                }
                retval.merge(tmp_digest);
            }
            return retval;
        }

        /// <summary> Sends the new view and digest to all subgroup coordinors in coords. Each coord will in turn
        /// <ol>
        /// <li>cast the new view and digest to all the members of its subgroup (MergeView)
        /// <li>on reception of the view, if it is a MergeView, each member will set the digest and install
        /// the new view
        /// </ol>
        /// </summary>
        internal virtual void sendMergeView(System.Collections.ArrayList coords, MergeData combined_merge_data)
        {
            Message msg;
            GmsHDR hdr;
            Address coord;
            View v;
            Digest d;

            if (coords == null || combined_merge_data == null)
                return;
            v = combined_merge_data.view;
            d = combined_merge_data.digest;
            if (v == null || d == null)
            {
                gms.Stack.NCacheLog.Error("view or digest is null, cannot send consolidated merge view/digest");
                return;
            }

            for (int i = 0; i < coords.Count; i++)
            {
                coord = (Address)coords[i];
                msg = new Message(coord, null, null);
                hdr = new GmsHDR(GmsHDR.INSTALL_MERGE_VIEW);
                hdr.view = v;
                hdr.digest = d;
                hdr.merge_id = merge_id;
                msg.putHeader(HeaderType.GMS, hdr);
                gms.passDown(new Event(Event.MSG, msg));
            }
        }

        /// <summary> Send back a response containing view and digest to sender</summary>
        internal virtual void sendMergeResponse(Address sender, View view, Digest digest)
        {
            Message msg = new Message(sender, null, null);
            GmsHDR hdr = new GmsHDR(GmsHDR.MERGE_RSP);
            hdr.merge_id = merge_id;
            hdr.view = view;
            hdr.digest = digest;
            msg.putHeader(HeaderType.GMS, hdr);

            gms.Stack.NCacheLog.Debug("response=" + hdr);

            gms.passDown(new Event(Event.MSG, msg));
        }

        internal virtual void sendMergeRejectedResponse(Address sender)
        {
            Message msg = new Message(sender, null, null);
            GmsHDR hdr = new GmsHDR(GmsHDR.MERGE_RSP);
            hdr.merge_rejected = true;
            hdr.merge_id = merge_id;
            msg.putHeader(HeaderType.GMS, hdr);

            gms.Stack.NCacheLog.Debug("response=" + hdr);

            gms.passDown(new Event(Event.MSG, msg));
        }

        internal virtual void sendMergeCancelledMessage(System.Collections.ArrayList coords, object merge_id)
        {
            Message msg;
            GmsHDR hdr;
            Address coord;

            if (coords == null || merge_id == null)
            {
                gms.Stack.NCacheLog.Error("coords or merge_id == null");
                return;
            }
            for (int i = 0; i < coords.Count; i++)
            {
                coord = (Address)coords[i];
                msg = new Message(coord, null, null);
                hdr = new GmsHDR(GmsHDR.CANCEL_MERGE);
                hdr.merge_id = merge_id;
                msg.putHeader(HeaderType.GMS, hdr);
                gms.passDown(new Event(Event.MSG, msg));
            }
        }

        /// <summary>Removed rejected merge requests from merge_rsps and coords </summary>
        internal virtual void removeRejectedMergeRequests(System.Collections.ArrayList coords)
        {
            for (int i = merge_rsps.Count - 1; i >= 0; i--)
            {
                MergeData data = (MergeData)merge_rsps[i];
                if (data.merge_rejected)
                {
                    if (data.Sender != null && coords != null)
                        coords.Remove(data.Sender);

                    merge_rsps.RemoveAt(i);
                    gms.Stack.NCacheLog.Debug("removed element " + data);
                }
            }
        }


        internal void ReSaturateCluster()
        {

        }

        /* --------------------------------------- End of Private methods ------------------------------------- */

        /// <summary>
        /// Shoudl be called only incase of POR, this will perform same steps as that of ClientGMSImpl.join
        /// will call in findInitialMembers
        /// will perfmom determine coordinator
        /// will call join request
        /// 
        /// This is for the case when all nodes of a cluster are simoultaneously started and every node creates a seperate cluster
        /// after cluster startup this function will only make sure that they should join in.
        /// </summary>
        /// <param name="isPOR"></param>
        /// <rereturns>true was cluster health is ok</rereturns>
        internal bool CheckOwnClusterHealth(bool isPOR, int retryNumber)
        {
            if(gms.Stack.NCacheLog.IsInfoEnabled) gms.Stack.NCacheLog.Info("CoordGmsImpl.Join()", "CheckOwnClusterHealth - Retry Number: " + retryNumber.ToString());
            JoinRsp rsp;
            Digest tmp_digest;
            //Console.WriteLine("initial_mbrs.Count = " + initial_mbrs.Count + "\tgms.members.Members.Count: " + gms.members.Members.Count + (gms.members.Members.Count > 2) + !(initial_mbrs.Count > 2));
            if(retryNumber !=1 )
            {
                if (isPOR)
                {
                    if (!((initial_mbrs.Count > 2) && (gms.members.Members.Count <= 2)))
                    {
                        return true;
                    }
                }
                else
                {
                    FindAliveMembers();
                    if (!((initial_mbrs.Count > 1) && (gms.members.Members.Count == 1)))
                    {
                        return true;
                    }
                }
            }
           
            if(retryNumber > 1)
                Util.Util.sleep(gms._castViewChangeTimeOut / 2);


            int findAliveMembersRetry = 1;
            int retryCount = 3;

            try
            {
                while (!leaving)
                {
                    gms.Stack.NCacheLog.CriticalInfo("CoordGmsImpl.Join", "CheckOwnClusterHealth - Retry Count: " + retryCount.ToString());
                    ArrayList initMembers = FindAliveMembers();
                    join_promise.Reset();
                    if (initMembers.Count == 0)
                    {
                        findAliveMembersRetry--;
                        if (findAliveMembersRetry <= 0)
                            return true;
                        Util.Util.sleep(gms.join_retry_timeout);
                        initial_mbrs_received = false;
                        continue;
                    }

                    //This will determine that coord that is already a coord of a cluster or the one with the lowest IP
                    Address coord = determineCoord(initMembers);
                    
                    if (coord == null)
                    {
                        Util.Util.sleep(gms.join_retry_timeout);
                        continue;
                    }
                    else if (coord.Equals(gms.local_addr))
                        return true;
                    else
                    {
                        sendJoinMessage(coord, gms.local_addr, gms.subGroup_addr);
                        rsp = (JoinRsp)join_promise.WaitResult(gms._castViewChangeTimeOut);
                    }
                    

                    if (rsp == null)
                    {
                        gms.Stack.NCacheLog.CriticalInfo("CoordGmsImpl.Join()", "Reply was NULL");
                        retryCount--;
                        if (retryCount <= 0)
                            return true;

                        Util.Util.sleep(gms.join_timeout);
                        initial_mbrs_received = false;
                        continue;
                    }
                    else if (rsp.JoinResult == JoinResult.Rejected)
                    {
                        gms.Stack.NCacheLog.CriticalInfo("CoordGmsImpl.Join()", "Reply: JoinResult.Rejected");
                        return true;
                    }
                    else if (rsp.JoinResult == JoinResult.MembershipChangeAlreadyInProgress)
                    {
                        gms.Stack.NCacheLog.CriticalInfo("CoordGmsImpl.Join()", "Reply: JoinResult.MembershipChangeAlreadyInProgress");
                        Util.Util.sleep(gms.join_timeout);
                        continue;
                    }
                    else
                    {
                        tmp_digest = rsp.Digest;
                        if (tmp_digest != null)
                        {
                            tmp_digest.incrementHighSeqno(coord); // see DESIGN for an explanantion
                            gms.Stack.NCacheLog.Debug("CoordGmsImpl.Join()", "digest is " + tmp_digest);
                            gms.Digest = tmp_digest;
                        }
                        else
                            gms.Stack.NCacheLog.Error("CoordGmsImpl.Join()", "digest of JOIN response is null");

                        // 2. Install view
                        gms.Stack.NCacheLog.Debug("CoordGmsImpl.Join()", "[" + gms.local_addr + "]: JoinRsp=" + rsp.View + " [size=" + rsp.View.size() + "]\n\n");

                        if (rsp.View != null)
                        {

                            if (!installView(rsp.View, isPOR))
                            {
                                gms.Stack.NCacheLog.Error("CoordGmsImpl.Join()", "view installation failed, retrying to join group");
                                return true;
                            }
                            gms.Stack.IsOperational = true;

                            return false;
                        }
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                gms.Stack.NCacheLog.CriticalInfo("CoordGmsImpl.Join()", ex.ToString());
            }
            return true;

        }

        /// <summary> Called by join(). Installs the view returned by calling Coord.handleJoin() and
        /// becomes coordinator.
		/// </summary>
		private bool installView(View new_view, bool isPOR)
		{
            if (isPOR)
            {
                Address replica = (Address)gms.members.Members[1];
                SendCheckClusterHealth(replica, new_view.Coordinator); 
            }


			ArrayList mems = new_view.Members;
            gms.Stack.NCacheLog.Debug("pb.ClientGmsImpl.installView()",   "new_view=" + new_view);
			if (gms.local_addr == null || mems == null || !mems.Contains(gms.local_addr))
			{
                gms.Stack.NCacheLog.Error("pb.ClientGmsImpl.installView()",   "I (" + gms.local_addr + ") am not member of " + Global.CollectionToString(mems) + ", will not install view");
				return false;
			}

            //Cast view to the replica node as well
			gms.installView(new_view);
            //gms.castViewChange(new_view);
			gms.becomeParticipant();
			gms.Stack.IsOperational = true;

            Util.Util.sleep(gms.join_retry_timeout);
			return true;
		}

        /// <summary> Pings initial members. Removes self before returning vector of initial members.
        /// Uses IP multicast or gossiping, depending on parameters.
        /// </summary>
        internal ArrayList FindAliveMembers()
        {
            PingRsp ping_rsp;
            initial_mbrs.Clear();
            initial_mbrs_received = false;
            lock (initial_mbrs.SyncRoot)
            {
                gms.passDown(new Event(Event.FIND_INITIAL_MBRS));

                // the initial_mbrs_received flag is needed when passDown() is executed on the same thread, so when
                // it returns, a response might actually have been received (even though the initial_mbrs might still be empty)
                if (initial_mbrs_received == false)
                {
                    try
                    {
                        System.Threading.Monitor.Wait(initial_mbrs.SyncRoot);
                    }
                    catch (System.Exception ex)
                    {
                        gms.Stack.NCacheLog.Error("COORDGmsImpl.findInitialMembers", ex.Message);
                    }
                }

                for (int i = 0; i < initial_mbrs.Count; i++)
                {
                    ping_rsp = (PingRsp)initial_mbrs[i];
                    if (ping_rsp.OwnAddress != null && gms.local_addr != null && ping_rsp.OwnAddress.Equals(gms.local_addr))
                    {
                        //initial_mbrs.RemoveAt(i);
                        break;
                    }
                    if (!ping_rsp.IsStarted) initial_mbrs.RemoveAt(i);
                } 
            }

            return initial_mbrs;
        }

        public override bool handleUpEvent(Event evt)
        {
            ArrayList tmp;

            switch (evt.Type)
            {
                case Event.FIND_INITIAL_MBRS_OK:
                    tmp = (ArrayList)evt.Arg;
                    System.Threading.Monitor.Pulse(initial_mbrs.SyncRoot);
                    lock (initial_mbrs.SyncRoot)
                    {
                        if (tmp != null && tmp.Count > 0)
                            for (int i = 0; i < tmp.Count; i++)
                                initial_mbrs.Add(tmp[i]);
                        initial_mbrs_received = true;
                    }
                    return false; // don't pass up the stack
            }
            return true;
        }


        internal virtual Address determineCoord(ArrayList mbrs)
        {
            if (mbrs == null || mbrs.Count < 1)
                return null;

            Address winner = null;
            int max_votecast = 0;
            Hashtable votes = Hashtable.Synchronized(new Hashtable(11));
            for (int i = 0; i < mbrs.Count; i++)
            {
                PingRsp mbr = (PingRsp)mbrs[i];
                if (mbr.CoordAddress != null)
                {
                    if (!votes.ContainsKey(mbr.CoordAddress))
                        votes[mbr.CoordAddress] = mbr.HasJoined ? 1000 : 1;
                    else
                    {
                        int count = ((int)votes[mbr.CoordAddress]);
                        votes[mbr.CoordAddress] = (int)(count + 1);
                    }

                    /// Find the maximum vote cast value. This will be used to resolve a
                    /// tie later on.
                    if (((int)votes[mbr.CoordAddress]) > max_votecast)
                        max_votecast = ((int)votes[mbr.CoordAddress]);

                  
                    gms.Stack.NCacheLog.CriticalInfo("CoordGmsImpl.DetermineCoord", "Owner " + mbr.OwnAddress + " -- CoordAddress " + mbr.CoordAddress + " -- Vote " + (int)votes[mbr.CoordAddress]);

                }
            }


            /// Collect all the candidates with the highest but similar vote count.
            /// Ideally there should only be one.
            ArrayList candidates = new ArrayList(votes.Count);
            for (IDictionaryEnumerator e = votes.GetEnumerator(); e.MoveNext(); )
            {
                if (((int)e.Value) == max_votecast)
                {
                    candidates.Add(e.Key);
                }
            }

            candidates.Sort();
            if (winner == null)
            {
                winner = (Address)candidates[0];
            }

            if (candidates.Count > 1)
                gms.Stack.NCacheLog.Warn("pb.CoordGmsImpl.determineCoord()", "there was more than 1 candidate for coordinator: " + Global.CollectionToString(candidates));
            gms.Stack.NCacheLog.CriticalInfo("CoordGmsImpl.DetermineCoord", "election winner: " + winner + " with votes " + max_votecast);

            return winner;
        }


        internal virtual void sendJoinMessage(Address coord, Address mbr, string subGroup_name)
        {          
            Message msg;
            GmsHDR hdr;

            msg = new Message(coord, null, null);
            hdr = new GmsHDR(GmsHDR.JOIN_REQ, mbr, subGroup_name);
            hdr.GMSId = gms.unique_id;
            msg.putHeader(HeaderType.GMS, hdr);
            gms.passDown(new Event(Event.MSG_URGENT, msg, Priority.High));
        }


        internal virtual void SendCheckClusterHealth(Address destination, Address coord)
        {
            Message msg;
            GmsHDR hdr;

            msg = new Message(destination, null, null);
            hdr = new GmsHDR(GmsHDR.RE_CHECK_CLUSTER_HEALTH, coord);
            hdr.GMSId = gms.unique_id;
            msg.putHeader(HeaderType.GMS, hdr);
            gms.passDown(new Event(Event.MSG_URGENT, msg, Priority.High));
        }
    }
}

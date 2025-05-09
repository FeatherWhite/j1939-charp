/*
 * Created by SharpDevelop.
 * User: dbw
 * Date: 7/20/2015
 * Time: 2:11 PM
 *
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.IO;
using System.Xml;

namespace Triumph.J1939
{
    public partial class J1939TP
    {
        private byte TP_CM_ABORT = 0xFF;
        private byte TP_CM_BAM = 0x20;
        private byte TP_CM_EndOfMsgACK = 0x13;
        private byte TP_CM_CTS = 0x11;
        private byte TP_CM_RTS = 0x10;
        private byte ACKNOWLEDGEMENT_PGN_SUPPORTED = 0x0;
        private byte ACKNOWLEDGEMENT_PGN_NOT_SUPPORTED = 0x1;
        private byte ACKNOWLEDGEMENT_PGN_ACCESS_DENIED = 0x2;
        private byte ACKNOWLEDGEMENT_PGN_BUSY = 0x3;

        public delegate bool SendCanFunc(uint arbitrationId, uint channel, byte[] elems);

        public SendCanFunc SendCan;
        public uint Channel { get; set; } = 0;
        private byte PC_SA { get; set; } = 0xF9;

        private J1939Model J1939TpTx;
        private List<J1939Model> j1939TpRxList = null;

        private const uint MEM_CMD_ERASE = 0;
        private const uint MEM_CMD_READ = 1;
        private const uint MEM_CMD_WRITE = 2;
        private const uint MEM_CMD_STATUS_REQ = 3;
        private const uint MEM_CMD_OP_COMPLETE = 4;
        private const uint MEM_CMD_OP_FAIL = 5;
        private const uint MEM_CMD_BOOT_LOAD = 6;
        private const uint MEM_CMD_EDCP_GEN = 7;

        // memory spaces
        private const uint MEM_SPACE_FLASH = 0;
        private const uint MEM_SPACE_EEPROM = 1;
        private const uint MEM_SPACE_APP_CKS = 2;
        private const uint MEM_SPACE_PN_SN = 3;

        // globals
        private string strMffReply = "";
        public List<int> SrcAddrList = null;
        public int DestAddr = 0x80;

        public J1939TP()
        {
        }

        public void SendPoll()
        {
            if ((J1939TpTx != null) && (J1939TpTx.state == J1939Model.STATE_TX_DATA))
            {
                if (DateTime.Now >= J1939TpTx.timestamp.AddMilliseconds(1))
                {
                    // time to transmit next packet
                    J1939msg msg = new J1939msg();

                    msg.prio = 0x18;
                    msg.pgn = 0xEB00;
                    msg.da = J1939TpTx.da;
                    msg.sa = PC_SA;
                    msg.dlc = 8;
                    msg.data = new byte[8];
                    msg.data[0] = (byte)(J1939TpTx.packets + 1);
                    for (byte i = 0; (i < 7) && (J1939TpTx.bytes < J1939TpTx.exp_bytes); i++, J1939TpTx.bytes++)
                    {
                        msg.data[i + 1] = J1939TpTx.data[J1939TpTx.bytes];
                    }
                    TransmitJ1939(msg);
                    J1939TpTx.packets++;
                    if (J1939TpTx.packets == J1939TpTx.exp_packets)
                    {
                        // done...
                        J1939TpTx.state = J1939Model.STATE_IDLE;
                    }
                    else
                    {
                        // more packets to transmit
                        J1939TpTx.timestamp = DateTime.Now;
                    }
                }
            }
            else
            {
                Thread.Sleep(5);
            }
        }

        private void TransmitJ1939(J1939msg msg)
        {
            if (msg.dlc <= 8)
            {
                uint canId = 0;

                // form identifier
                canId = (uint)msg.prio << 24;
                canId |= (uint)msg.pgn << 8;
                if ((msg.pgn >= 0xC000) && (msg.pgn < 0xF000))
                {
                    canId |= (uint)msg.da << 8;
                }
                canId |= (uint)msg.sa;

                byte[] DATA = new byte[8];

                for (byte i = 0; i < msg.dlc; i++)
                {
                    DATA[i] = msg.data[i];
                }

                // write
                SendCan(canId, Channel, DATA);
            }
            else
            {                       // must be multi-packet
                J1939TpTx.pgn = (ushort)msg.pgn;
                J1939TpTx.da = msg.da;
                J1939TpTx.exp_bytes = (ushort)msg.dlc;
                J1939TpTx.bytes = 0;
                J1939TpTx.exp_packets = (byte)(msg.dlc / 7);
                if ((msg.dlc % 7) > 0)
                {
                    J1939TpTx.exp_packets++;
                }
                J1939TpTx.packets = 0;
                // copy the data
                J1939TpTx.data = new byte[256];
                for (ushort i = 0; i < msg.dlc; i++)
                {
                    J1939TpTx.data[i] = msg.data[i];
                }

                // form the TPCM message
                J1939msg jmsg = new J1939msg();
                jmsg.prio = 0x18;
                jmsg.pgn = 0xEC00;
                jmsg.da = J1939TpTx.da;
                jmsg.sa = PC_SA;
                jmsg.dlc = 8;
                jmsg.data = new byte[8];
                jmsg.data[1] = (byte)J1939TpTx.exp_bytes;
                jmsg.data[2] = (byte)(J1939TpTx.exp_bytes >> 8);
                jmsg.data[3] = J1939TpTx.exp_packets;
                jmsg.data[4] = 0xff;
                jmsg.data[5] = (byte)J1939TpTx.pgn;
                jmsg.data[6] = (byte)(J1939TpTx.pgn >> 8);
                jmsg.data[7] = 0x00;
                if (jmsg.da == 0xff)
                {                     // send BAM
                    jmsg.data[0] = 0x20;
                    J1939TpTx.state = J1939Model.STATE_TX_DATA;
                }
                else
                {                                     // send RTS
                    jmsg.data[0] = 0x10;
                    J1939TpTx.state = J1939Model.STATE_WAIT_CTS;
                }
                J1939TpTx.timestamp = DateTime.Now;
                TransmitJ1939(jmsg);
            }
        }

        private const int MEM_ST_PROCEED = 0;
        private const int MEM_ST_BUSY = 1;
        private const int MEM_ST_RES_1 = 2;
        private const int MEM_ST_RES_2 = 3;
        private const int MEM_ST_OP_COMP = 4;
        private const int MEM_ST_OP_FAIL = 5;
        private const int MEM_ST_RES_3 = 6;
        private const int MEM_ST_RES_4 = 7;

        private int GatewayCheckSum = 0;

        public void ReceivePoll(uint receiveId, byte[] Data)
        {
            J1939msg jmsg = new J1939msg();
            jmsg.prio = (byte)(receiveId >> 24);
            jmsg.pgn = (ushort)(receiveId >> 8);
            if ((jmsg.pgn >= 0xC000) && (jmsg.pgn < 0xF000))
            {
                jmsg.pgn &= 0xFF00;
                jmsg.da = (byte)(receiveId >> 8);
            }
            jmsg.sa = (byte)receiveId;
            jmsg.dlc = 8;
            for (byte i = 0; i < jmsg.dlc; i++)
            {
                jmsg.data[i] = Data[i];
            }
            // call the message handler
            RxMsgHandler(jmsg);
            Thread.Sleep(5);
        }

        private void RxMsgHandler(J1939msg rxmsg)
        {
            switch (rxmsg.pgn)
            {
                case 0xEC00:                                                        // MCAN_PGN_TP_CM
                    if ((rxmsg.da == PC_SA) || (rxmsg.da == TP_CM_ABORT))
                    {               // address for MFF transfers
                        if ((rxmsg.data[0] == TP_CM_RTS) || (rxmsg.data[0] == TP_CM_BAM))
                        {    // received RTS
                            J1939Model rxTpMsg = FindRxTp(rxmsg.sa);  // use old Tp
                            if (rxTpMsg == null)
                            {
                                rxTpMsg = FindRxTp(0); // allocate new Tp
                            }
                            if (rxTpMsg != null)
                            {
                                rxTpMsg.exp_bytes = rxmsg.data[1];
                                rxTpMsg.exp_packets = rxmsg.data[3];
                                rxTpMsg.sa = rxmsg.sa;
                                rxTpMsg.da = rxmsg.da;
                                rxTpMsg.pgn = (uint)rxmsg.data[5] + (uint)rxmsg.data[6] * 0x100;
                                SendCTS(rxmsg);                                         // transmit CTS
                            }
                        }
                        if ((rxmsg.data[0] == TP_CM_CTS) && (J1939TpTx.state == J1939Model.STATE_WAIT_CTS))
                        {
                            // we are expecting this CTS
                            // start sending data packets
                            J1939TpTx.state = J1939Model.STATE_TX_DATA;
                            J1939TpTx.timestamp = DateTime.Now.AddMilliseconds(-10);
                        }
                    }
                    // TP_CM
                    if (rxmsg.da == PC_SA)
                    {
                        if ((rxmsg.data[0] == TP_CM_CTS) && (J1939TpTx.state == J1939Model.STATE_WAIT_CTS))
                        {
                            // we are expecting this CTS
                            // start sending data packets
                            J1939TpTx.state = J1939Model.STATE_TX_DATA;
                            J1939TpTx.timestamp = DateTime.Now.AddMilliseconds(-10);
                        }
                    }
                    break;

                case 0xEB00:                                 						// receive transfer protocol message
                    if ((rxmsg.da == PC_SA) || (rxmsg.da == TP_CM_ABORT))
                    {                   // address for MFF transfers
                        J1939Model rxTpMsg = FindRxTp(rxmsg.sa);
                        if (rxTpMsg != null)
                        {
                            int seqNum = rxmsg.data[0];
                            if ((seqNum >= 1) && (seqNum <= rxTpMsg.exp_packets))
                            {
                                int Start = (seqNum - 1) * 7;
                                int cnt;
                                for (cnt = 1; cnt < 8; cnt++)
                                {
                                    rxTpMsg.ReceiveBuff[Start + cnt - 1] = rxmsg.data[cnt];
                                }
                            }
                            if (seqNum == rxTpMsg.exp_packets)
                            {
                                // end of transfer protocol
                                if (rxTpMsg.da == PC_SA)
                                {
                                    SendAckTp(rxTpMsg); // send ack for transfer complete
                                }
                                CanRxMsgSorter(rxTpMsg);
                            }
                        }
                    }
                    break;
                //case MCAN_PGN_DM15:
                //    gwProcessDM15(rxmsg);
                //    break;
                case 0xEE00:                            // MCAN_PGN_ADDR_CLAIM
                    if (SrcAddrList.Contains(rxmsg.sa) == false)
                    {
                        SrcAddrList.Add(rxmsg.sa);
                    }
                    break;

                default:
                    break;
            }
        }

        private void CanRxMsgSorter(J1939Model rxTpMsg)
        {
            /*! /note This function is recursive to 1 level...be careful!! */
            // make sure this is for us
            if ((rxTpMsg.da == MCAN_SA_GLOBAL) || (rxTpMsg.da == PC_SA))
            {
                // route to the appropriate handler
                switch (rxTpMsg.pgn)
                {
                    case MCAN_PGN_COMPONENT_ID:
                        gwProcessCompID(rxTpMsg);
                        break;

                    default:
                        break;
                }
                /* pass the message to the application layer via callback function */
                if (rxTpMsg.sa == gw_response_mod)
                {
                    byte[] byteData = CopyToLength(rxTpMsg.ReceiveBuff, rxTpMsg.exp_bytes);
                    string strData = Encoding.UTF8.GetString(byteData);    // convert byte[] to string
                    strMffReply = strData;
                    if (strData.StartsWith("H"))
                    {
                        MsgToProduct(rxTpMsg.sa.ToString("X2"), "PGN", strData);
                    }
                }
            }
        }

        // find j1939 transfer protocol message by source address.
        // source address 0 is unused.
        private J1939Model FindRxTp(byte srcAddr)
        {
            foreach (J1939Model msg in j1939TpRxList)
            {
                if (msg.sa == srcAddr)
                {
                    return (msg);
                }
            }
            return (null);                      // dstAddr not found
        }

        private void gwProcessCompID(J1939Model rxTpMsg)
        {
            if ((rxTpMsg.sa == gw_response_mod) && (gw_response == GW_RESP_COMP_ID))
            {
                /* we were expecting this response */
                /* make sure it is properly terminated */
                byte[] byteData = CopyToLength(rxTpMsg.ReceiveBuff, rxTpMsg.exp_bytes);
                string strData = Encoding.UTF8.GetString(byteData);    		// convert byte[] to string
                strMffReply = strData;
                string srcAddr = rxTpMsg.sa.ToString("X02");
                string strMsg = "HF9" + srcAddr + "0303;0;" + strData + ";00\n";
                MsgToProduct(rxTpMsg.sa.ToString("X2"), "PGN", strMsg);
                gw_response = GW_RESP_NONE;
            }
        }

        private void gwProcessDM15(J1939msg rxmsg)
        {
            /* make sure we got a response from the correct module */
            if (rxmsg.sa == gw_response_mod)
            {
                int status = (rxmsg.data[0] & 0x0E) >> 1;
                switch (gw_response)
                {
                    //					case GW_RESP_EXIT_TO_BOOT:
                    //						if (status == MEM_ST_OP_COMP) {
                    //							/* delay some time to allow the module to reboot */
                    //							gw_delay = 500;
                    //						}
                    //						break;
                    case GW_RESP_ERASE:
                        if (status == MEM_ST_OP_COMP)
                        {
                            /* now we need to write the memory */
                            SendDM14(gw_response_mod, gw_addr, 16, MEM_WRITE, MEM_SPACE_FLASH);
                            gw_response = GW_RESP_WRITE;
                            gw_tmr_limit = 5;               // 0.5 seconds
                        }
                        else
                        {
                            gw_response = GW_RESP_NONE;
                        }
                        break;

                    case GW_RESP_WRITE:
                        if (status == MEM_ST_PROCEED)
                        {
                            /* send the DM16 */
                            J1939msg msg = new J1939msg();
                            msg.prio = 0x00;
                            msg.pgn = MCAN_PGN_DM16;
                            msg.da = (byte)gw_response_mod;
                            msg.sa = PC_SA;
                            msg.dlc = 16;
                            for (byte i = 0; i < 16; i++)
                            {
                                msg.data[i] = (byte)gw_buff[i];
                            }
                            TransmitJ1939(msg);
                        }
                        else if (status == MEM_ST_OP_COMP)
                        {
                            /* write operation is complete, send the response */
                            gw_response = GW_RESP_NONE;
                            MsgToProduct(rxmsg.sa.ToString("X2"), "PGN", "HF980;W;\n");
                        }
                        else
                        {
                            MsgToProduct(rxmsg.sa.ToString("X2"), "PGN", "\n");
                            gw_response = GW_RESP_NONE;
                        }
                        break;

                    case GW_RESP_CALC_CKS:
                        if (status == MEM_ST_OP_COMP)
                        {
                            /* extract the checksum */
                            GatewayCheckSum = rxmsg.data[1];
                            GatewayCheckSum |= (int)(rxmsg.data[2]) << 8;
                            string strMsg = "HF9" + gw_response_mod.ToString("X02") + "0305;" + GatewayCheckSum.ToString("X4") + ";00\n";
                            MsgToProduct(rxmsg.sa.ToString("X2"), "PGN", strMsg);
                        }
                        break;

                    case GW_RESP_CKS_WRITE:
                        if (status == MEM_ST_PROCEED)
                        {
                            /* send DM16 */
                            J1939msg msg = new J1939msg();
                            msg.prio = 0x00;
                            msg.pgn = MCAN_PGN_DM16;
                            msg.da = (byte)gw_response_mod;
                            msg.sa = PC_SA;
                            msg.dlc = 2;
                            msg.data[0] = (byte)(GatewayCheckSum & 0xff);
                            msg.data[1] = (byte)(GatewayCheckSum >> 8);
                            TransmitJ1939(msg);
                        }
                        else if (status == MEM_ST_OP_COMP)
                        {
                            MsgToProduct(rxmsg.sa.ToString("X2"), "PGN", "C\n");
                            gw_response = GW_RESP_NONE;
                        }
                        else
                        {
                            MsgToProduct(rxmsg.sa.ToString("X2"), "PGN", "\n");
                            gw_response = GW_RESP_NONE;
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        private void SendCTS(J1939msg rxmsg)
        {
            J1939msg msg = new J1939msg();
            msg.prio = 0x18;
            msg.pgn = 0xEC00;
            msg.da = (byte)DestAddr;
            msg.sa = PC_SA;
            msg.dlc = 8;
            msg.data[0] = 0x11;
            msg.data[1] = rxmsg.data[3];
            msg.data[2] = 1;
            msg.data[3] = 0xff;
            msg.data[4] = 0xff;
            msg.data[5] = rxmsg.data[5];
            msg.data[6] = rxmsg.data[6];
            msg.data[7] = 0;
            TransmitJ1939(msg);
        }

        private void SendAckTp(J1939Model rxTpMsg)
        {
            J1939msg msg = new J1939msg();
            msg.prio = 0x18;
            msg.pgn = 0xEC00;
            msg.da = (byte)DestAddr;
            msg.sa = PC_SA;
            msg.dlc = 8;
            msg.data[0] = 0x13;
            msg.data[1] = (byte)rxTpMsg.exp_bytes;
            msg.data[2] = 0;
            msg.data[3] = rxTpMsg.exp_packets;
            msg.data[4] = 0xFF;
            msg.data[5] = 0;
            msg.data[6] = 0xEF;
            msg.data[7] = 0;
            TransmitJ1939(msg);
        }
    }
}
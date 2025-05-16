using System;
using System.Collections.Generic;
using System.Text;

namespace Triumph.J1939
{
    public enum PGNCode : uint
    {
        ADDRESS_DELETE = 0x000002U, /* NOT SAE J1939 standard. If you find the correct PGN number, please do a pull request */
        REQUEST = 0x00EA00U,
        ACKNOWLEDGEMENT = 0x00E800U,
        TP_CM = 0x00EC00U,
        TP_DT = 0x00EB00U,
        ADDRESS_CLAIMED = 0x00EE00U,
        PROPRIETARY_A = 0x00EF00U,
        COMMANDED_ADDRESS = 0x00FED8U,
        DM1 = 0x00FECAU,
        DM2 = 0x00FECBU,
        DM3 = 0x00FECCU,
        DM14 = 0x00D900U,
        DM15 = 0x00D800U,
        DM16 = 0x00D700U,
        SOFTWARE_IDENTIFICATION = 0x00FEDAU,
        ECU_IDENTIFICATION = 0x00FDC5U,
        COMPONENT_IDENTIFICATION = 0x00FEEBU,
        AUXILIARY_VALVE_ESTIMATED_FLOW_0 = 0x00FE10U,
        AUXILIARY_VALVE_ESTIMATED_FLOW_1 = 0x00FE11U,
        AUXILIARY_VALVE_ESTIMATED_FLOW_2 = 0x00FE12U,
        AUXILIARY_VALVE_ESTIMATED_FLOW_3 = 0x00FE13U,
        AUXILIARY_VALVE_ESTIMATED_FLOW_4 = 0x00FE14U,
        AUXILIARY_VALVE_ESTIMATED_FLOW_5 = 0x00FE15U,
        AUXILIARY_VALVE_ESTIMATED_FLOW_6 = 0x00FE16U,
        AUXILIARY_VALVE_ESTIMATED_FLOW_7 = 0x00FE17U,
        AUXILIARY_VALVE_ESTIMATED_FLOW_8 = 0x00FE18U,
        AUXILIARY_VALVE_ESTIMATED_FLOW_9 = 0x00FE19U,
        AUXILIARY_VALVE_ESTIMATED_FLOW_10 = 0x00FE1AU,
        AUXILIARY_VALVE_ESTIMATED_FLOW_11 = 0x00FE1BU,
        AUXILIARY_VALVE_ESTIMATED_FLOW_12 = 0x00FE1CU,
        AUXILIARY_VALVE_ESTIMATED_FLOW_13 = 0x00FE1DU,
        AUXILIARY_VALVE_ESTIMATED_FLOW_14 = 0x00FE1EU,
        AUXILIARY_VALVE_ESTIMATED_FLOW_15 = 0x00FE1FU,
        AUXILIARY_VALVE_MEASURED_POSITION_0 = 0x00FF20U,
        AUXILIARY_VALVE_MEASURED_POSITION_1 = 0x00FF21U,
        AUXILIARY_VALVE_MEASURED_POSITION_2 = 0x00FF22U,
        AUXILIARY_VALVE_MEASURED_POSITION_3 = 0x00FF23U,
        AUXILIARY_VALVE_MEASURED_POSITION_4 = 0x00FF24U,
        AUXILIARY_VALVE_MEASURED_POSITION_5 = 0x00FF25U,
        AUXILIARY_VALVE_MEASURED_POSITION_6 = 0x00FF26U,
        AUXILIARY_VALVE_MEASURED_POSITION_7 = 0x00FF27U,
        AUXILIARY_VALVE_MEASURED_POSITION_8 = 0x00FF28U,
        AUXILIARY_VALVE_MEASURED_POSITION_9 = 0x00FF29U,
        AUXILIARY_VALVE_MEASURED_POSITION_10 = 0x00FF2AU,
        AUXILIARY_VALVE_MEASURED_POSITION_11 = 0x00FF2BU,
        AUXILIARY_VALVE_MEASURED_POSITION_12 = 0x00FF2CU,
        AUXILIARY_VALVE_MEASURED_POSITION_13 = 0x00FF2DU,
        AUXILIARY_VALVE_MEASURED_POSITION_14 = 0x00FF2EU,
        AUXILIARY_VALVE_MEASURED_POSITION_15 = 0x00FF2FU,
        AUXILIARY_VALVE_COMMAND_0 = 0x00FE30U,
        AUXILIARY_VALVE_COMMAND_1 = 0x00FE31U,
        AUXILIARY_VALVE_COMMAND_2 = 0x00FE32U,
        AUXILIARY_VALVE_COMMAND_3 = 0x00FE33U,
        AUXILIARY_VALVE_COMMAND_4 = 0x00FE34U,
        AUXILIARY_VALVE_COMMAND_5 = 0x00FE35U,
        AUXILIARY_VALVE_COMMAND_6 = 0x00FE36U,
        AUXILIARY_VALVE_COMMAND_7 = 0x00FE37U,
        AUXILIARY_VALVE_COMMAND_8 = 0x00FE38U,
        AUXILIARY_VALVE_COMMAND_9 = 0x00FE39U,
        AUXILIARY_VALVE_COMMAND_10 = 0x00FE3AU,
        AUXILIARY_VALVE_COMMAND_11 = 0x00FE3BU,
        AUXILIARY_VALVE_COMMAND_12 = 0x00FE3CU,
        AUXILIARY_VALVE_COMMAND_13 = 0x00FE3DU,
        AUXILIARY_VALVE_COMMAND_14 = 0x00FE3EU,
        AUXILIARY_VALVE_COMMAND_15 = 0x00FE3FU,
        GENERAL_PURPOSE_VALVE_ESTIMATED_FLOW = 0x00C600U,
        ENGINE_HOURS_65253 = 0x00FE3FU,
        ENGINE_TEMPERATURE_1_65262 = 0x00FEEEU,
        VEHICLE_ELECTRICAL_POWER_1_65271 = 0x00FEF7U,
        ELECTRONIC_ENGINE_CONTROLLER_1_61444 = 0x00F004U,
        COLD_START_AIDS_64966 = 0x00FDC6U,
        FUEL_CONSUMPTION_65257 = 0x00FEE9U,
        FUEL_ECONOMY_65266 = 0x00FEF2U,
        ENGINE_FLUIDS_LEVEL_PRESSURE_1_65263 = 0x00FEEFU,
        ELECTRONIC_ENGINE_CONTROLLER_2_61443 = 0x00F003U,
        AMBIENT_CONDITIONS_65269 = 0x00FEF5U,
        ENGINE_FUEL_LUBE_SYSTEMS_65130 = 0x00FE6AU,
        AUXILIARY_ANALOG_INFORMATION_65164 = 0x00FE8CU,
        AFTERTREATMENT_1_DEF_TANK_1_65110 = 0x00FE56U,
        SHUTDOWN_65252 = 0x00FEE4U,
        ELECTRONIC_ENGINE_CONTROLLER_3_65247 = 0x00FEDFU,
        ENGINE_FLUIDS_LEVEL_PRESSURE_12_64735 = 0x00FCDFU,
        INTAKE_MANIFOLD_INFO_1_65190 = 0x00FEA6U,
        DASH_DISPLAY_65276 = 0x00FEFCU,
        DIRECT_LAMP_CONTROL_COMMAND_1_64775 = 0x00FD07U,
        TORQUE_SPEED_CONTROL_1_0 = 0x000000U,
        ELECTRONIC_BRAKE_CONTROLLER_1_61441 = 0x00F001U,
        PROPRIETARY_B_START = 0x00FF00U,
        PROPRIETARY_B_END = 0x00FFFFU,
        // Same as proprietary B but with DP (data page = 1)
        PROPRIETARY_B2_START = 0x01FF00U, /* This range is not officially support by the standard */
        PROPRIETARY_B2_END = 0x01FFFFU   /* but it is very commonly used, so it's supported here */

        /* Add more here */
    }

    public class ParameterGroupNumber
    {
        public byte dataPage;
        public byte pduFormat;
        public byte pduSpecific;

        public const byte NULL = 0xFE;
        public const byte GLOBAL = 0xFF;
        public ParameterGroupNumber()
        {
            
        }
        public ParameterGroupNumber(byte dataPage,byte pduFormat,byte pduSpecific)
        {
            this.dataPage = (byte)(dataPage & 0x01);
            this.pduFormat = (byte)(pduFormat & 0xFF);
            this.pduSpecific = (byte)(pduSpecific & 0xFF);
        }
        public bool IsPDU1Format()
        {
            return pduFormat >= 0 && pduFormat <= 239;
        }
        public bool IsPDU2Format()
        {
            return pduFormat >= 240 && pduFormat <= 255;
        }
        public void FromMessageId(MessageId mid)
        {
            if (mid == null)
            {
                throw new ArgumentException("The parameter mid must be an instance of MessageId");
            }
            // 假设 this.data_page, this.pdu_format, this.pdu_specific 是当前类的属性
            this.dataPage = Convert.ToByte((mid.ParameterGroupNumber >> 16) & 0x01);
            this.pduFormat = Convert.ToByte((mid.ParameterGroupNumber >> 8) & 0xFF);
            this.pduSpecific = Convert.ToByte(mid.ParameterGroupNumber & 0xFF);
        }
        public uint Value
        {
            /// <summary>
            /// Returns the value of the PGN
            /// </summary>
            get
            {
                return (uint)(dataPage << 16) | (uint)(pduFormat << 8) | pduSpecific;
            }
        }

    }

    public class MessageId
    {
        /// <summary>
        /// The CAN MessageId of a PDU.
        /// The MessageId consists of three parts:
        ///   * Priority
        ///   * Parameter Group Number
        ///   * Source Address
        /// </summary>

        public byte Priority { get; private set; }
        public uint ParameterGroupNumber { get; private set; }
        public byte SourceAddress { get; private set; }

        public const int CBFF = 0; // classical base frame format
        public const int CEFF = 1; // classical extended frame format
        public const int FBFF = 2; // flexible data rate base frame format
        public const int FEFF = 3; // flexible data rate extended frame format

        public MessageId(byte? priority = null, uint? parameterGroupNumber = null, 
            byte? sourceAddress = null, uint? canId = null)
        {
            if (canId.HasValue)
            {
                // Let the property canId parse the given value
                CanId = canId.Value;
            }
            else
            {
                Priority = Convert.ToByte((priority ?? 0) & 7);
                ParameterGroupNumber = Convert.ToUInt32((parameterGroupNumber ?? 0) & 0x3FFFF);
                SourceAddress = Convert.ToByte((sourceAddress ?? 0) & 0xFF);
            }
        }

        public uint CanId
        {
            get
            {
                /// <summary>
                /// Transforms the MessageId object to a 29 bit CAN-Id
                /// </summary>
                return (uint)(Priority << 26) | (ParameterGroupNumber << 8) | SourceAddress;
            }
            set
            {
                /// <summary>
                /// Fill the MessageId with the information given in the 29 bit CAN-Id
                /// </summary>
                SourceAddress = Convert.ToByte(value & 0xFF);
                ParameterGroupNumber = Convert.ToUInt32((value >> 8) & 0x3FFFF);
                Priority = Convert.ToByte((value >> 26) & 0x7);
            }
        }
    }

}
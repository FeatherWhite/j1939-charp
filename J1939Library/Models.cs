using System;
using System.Collections.Generic;
using System.Text;

namespace J1939Library
{
    public enum J1939StatusCode : ushort
    {
        SendOK = 0x00,
        SendError = 0x01,
        SendBusy = 0x02,
        SendTimeout = 0x03
    }
    public enum PGNCode : uint
    {
        PGN_ADDRESS_DELETE = 0x000002U, /* NOT SAE J1939 standard. If you find the correct PGN number, please do a pull request */
        PGN_REQUEST = 0x00EA00U,
        PGN_ACKNOWLEDGEMENT = 0x00E800U,
        PGN_TP_CM = 0x00EC00U,
        PGN_TP_DT = 0x00EB00U,
        PGN_ADDRESS_CLAIMED = 0x00EE00U,
        PGN_PROPRIETARY_A = 0x00EF00U,
        PGN_COMMANDED_ADDRESS = 0x00FED8U,
        PGN_DM1 = 0x00FECAU,
        PGN_DM2 = 0x00FECBU,
        PGN_DM3 = 0x00FECCU,
        PGN_DM14 = 0x00D900U,
        PGN_DM15 = 0x00D800U,
        PGN_DM16 = 0x00D700U,
        PGN_SOFTWARE_IDENTIFICATION = 0x00FEDAU,
        PGN_ECU_IDENTIFICATION = 0x00FDC5U,
        PGN_COMPONENT_IDENTIFICATION = 0x00FEEBU,
        PGN_AUXILIARY_VALVE_ESTIMATED_FLOW_0 = 0x00FE10U,
        PGN_AUXILIARY_VALVE_ESTIMATED_FLOW_1 = 0x00FE11U,
        PGN_AUXILIARY_VALVE_ESTIMATED_FLOW_2 = 0x00FE12U,
        PGN_AUXILIARY_VALVE_ESTIMATED_FLOW_3 = 0x00FE13U,
        PGN_AUXILIARY_VALVE_ESTIMATED_FLOW_4 = 0x00FE14U,
        PGN_AUXILIARY_VALVE_ESTIMATED_FLOW_5 = 0x00FE15U,
        PGN_AUXILIARY_VALVE_ESTIMATED_FLOW_6 = 0x00FE16U,
        PGN_AUXILIARY_VALVE_ESTIMATED_FLOW_7 = 0x00FE17U,
        PGN_AUXILIARY_VALVE_ESTIMATED_FLOW_8 = 0x00FE18U,
        PGN_AUXILIARY_VALVE_ESTIMATED_FLOW_9 = 0x00FE19U,
        PGN_AUXILIARY_VALVE_ESTIMATED_FLOW_10 = 0x00FE1AU,
        PGN_AUXILIARY_VALVE_ESTIMATED_FLOW_11 = 0x00FE1BU,
        PGN_AUXILIARY_VALVE_ESTIMATED_FLOW_12 = 0x00FE1CU,
        PGN_AUXILIARY_VALVE_ESTIMATED_FLOW_13 = 0x00FE1DU,
        PGN_AUXILIARY_VALVE_ESTIMATED_FLOW_14 = 0x00FE1EU,
        PGN_AUXILIARY_VALVE_ESTIMATED_FLOW_15 = 0x00FE1FU,
        PGN_AUXILIARY_VALVE_MEASURED_POSITION_0 = 0x00FF20U,
        PGN_AUXILIARY_VALVE_MEASURED_POSITION_1 = 0x00FF21U,
        PGN_AUXILIARY_VALVE_MEASURED_POSITION_2 = 0x00FF22U,
        PGN_AUXILIARY_VALVE_MEASURED_POSITION_3 = 0x00FF23U,
        PGN_AUXILIARY_VALVE_MEASURED_POSITION_4 = 0x00FF24U,
        PGN_AUXILIARY_VALVE_MEASURED_POSITION_5 = 0x00FF25U,
        PGN_AUXILIARY_VALVE_MEASURED_POSITION_6 = 0x00FF26U,
        PGN_AUXILIARY_VALVE_MEASURED_POSITION_7 = 0x00FF27U,
        PGN_AUXILIARY_VALVE_MEASURED_POSITION_8 = 0x00FF28U,
        PGN_AUXILIARY_VALVE_MEASURED_POSITION_9 = 0x00FF29U,
        PGN_AUXILIARY_VALVE_MEASURED_POSITION_10 = 0x00FF2AU,
        PGN_AUXILIARY_VALVE_MEASURED_POSITION_11 = 0x00FF2BU,
        PGN_AUXILIARY_VALVE_MEASURED_POSITION_12 = 0x00FF2CU,
        PGN_AUXILIARY_VALVE_MEASURED_POSITION_13 = 0x00FF2DU,
        PGN_AUXILIARY_VALVE_MEASURED_POSITION_14 = 0x00FF2EU,
        PGN_AUXILIARY_VALVE_MEASURED_POSITION_15 = 0x00FF2FU,
        PGN_AUXILIARY_VALVE_COMMAND_0 = 0x00FE30U,
        PGN_AUXILIARY_VALVE_COMMAND_1 = 0x00FE31U,
        PGN_AUXILIARY_VALVE_COMMAND_2 = 0x00FE32U,
        PGN_AUXILIARY_VALVE_COMMAND_3 = 0x00FE33U,
        PGN_AUXILIARY_VALVE_COMMAND_4 = 0x00FE34U,
        PGN_AUXILIARY_VALVE_COMMAND_5 = 0x00FE35U,
        PGN_AUXILIARY_VALVE_COMMAND_6 = 0x00FE36U,
        PGN_AUXILIARY_VALVE_COMMAND_7 = 0x00FE37U,
        PGN_AUXILIARY_VALVE_COMMAND_8 = 0x00FE38U,
        PGN_AUXILIARY_VALVE_COMMAND_9 = 0x00FE39U,
        PGN_AUXILIARY_VALVE_COMMAND_10 = 0x00FE3AU,
        PGN_AUXILIARY_VALVE_COMMAND_11 = 0x00FE3BU,
        PGN_AUXILIARY_VALVE_COMMAND_12 = 0x00FE3CU,
        PGN_AUXILIARY_VALVE_COMMAND_13 = 0x00FE3DU,
        PGN_AUXILIARY_VALVE_COMMAND_14 = 0x00FE3EU,
        PGN_AUXILIARY_VALVE_COMMAND_15 = 0x00FE3FU,
        PGN_GENERAL_PURPOSE_VALVE_ESTIMATED_FLOW = 0x00C600U,
        PGN_ENGINE_HOURS_65253 = 0x00FE3FU,
        PGN_ENGINE_TEMPERATURE_1_65262 = 0x00FEEEU,
        PGN_VEHICLE_ELECTRICAL_POWER_1_65271 = 0x00FEF7U,
        PGN_ELECTRONIC_ENGINE_CONTROLLER_1_61444 = 0x00F004U,
        PGN_COLD_START_AIDS_64966 = 0x00FDC6U,
        PGN_FUEL_CONSUMPTION_65257 = 0x00FEE9U,
        PGN_FUEL_ECONOMY_65266 = 0x00FEF2U,
        PGN_ENGINE_FLUIDS_LEVEL_PRESSURE_1_65263 = 0x00FEEFU,
        PGN_ELECTRONIC_ENGINE_CONTROLLER_2_61443 = 0x00F003U,
        PGN_AMBIENT_CONDITIONS_65269 = 0x00FEF5U,
        PGN_ENGINE_FUEL_LUBE_SYSTEMS_65130 = 0x00FE6AU,
        PGN_AUXILIARY_ANALOG_INFORMATION_65164 = 0x00FE8CU,
        PGN_AFTERTREATMENT_1_DEF_TANK_1_65110 = 0x00FE56U,
        PGN_SHUTDOWN_65252 = 0x00FEE4U,
        PGN_ELECTRONIC_ENGINE_CONTROLLER_3_65247 = 0x00FEDFU,
        PGN_ENGINE_FLUIDS_LEVEL_PRESSURE_12_64735 = 0x00FCDFU,
        PGN_INTAKE_MANIFOLD_INFO_1_65190 = 0x00FEA6U,
        PGN_DASH_DISPLAY_65276 = 0x00FEFCU,
        PGN_DIRECT_LAMP_CONTROL_COMMAND_1_64775 = 0x00FD07U,
        PGN_TORQUE_SPEED_CONTROL_1_0 = 0x000000U,
        PGN_ELECTRONIC_BRAKE_CONTROLLER_1_61441 = 0x00F001U,
        PGN_PROPRIETARY_B_START = 0x00FF00U,
        PGN_PROPRIETARY_B_END = 0x00FFFFU,
        // Same as proprietary B but with DP (data page = 1)
        PGN_PROPRIETARY_B2_START = 0x01FF00U, /* This range is not officially support by the standard */
        PGN_PROPRIETARY_B2_END = 0x01FFFFU   /* but it is very commonly used, so it's supported here */

        /* Add more here */
    }

    public class Constant
    {
        public const uint MAX_TP_DT = 1785;
        public const uint MAX_IDENTIFICATION = 30;
        public const uint MAX_DM_FIELD = 10;
        public const uint MAX_PROPRIETARY_A = 15;
        public const uint MAX_PROPRIETARY_B = 60;
        public const uint MAX_PROPRIETARY_B_PGNS = 2;
    }

    /// <summary>
    /// PGN: 0x00FEDA - Storing the software identification from the reading process
    /// </summary>
    public class SoftwareIdentification
    {
        public byte number_of_fields; /* How many numbers contains in the identifications array */
        public byte[] identifications = new byte[Constant.MAX_IDENTIFICATION];    /* This can be for example ASCII */
        public byte from_ecu_address;	/* From which ECU came this message */
    }

    /// <summary>
    /// PGN: 0x00FDC5 - Storing the ECU identification from the reading process
    /// </summary>
    public class ECUIdentification
    {
        public byte length_of_each_field;                   /* The real length of the fields - Not part of J1939 standard, only for the user */
        public byte[] ecu_part_number = new byte[Constant.MAX_IDENTIFICATION];    /* ASCII field */
        public byte[] ecu_serial_number = new byte[Constant.MAX_IDENTIFICATION];  /* ASCII field */
        public byte[] ecu_location = new byte[Constant.MAX_IDENTIFICATION];       /* ASCII field */
        public byte[] ecu_type = new byte[Constant.MAX_IDENTIFICATION];           /* ASCII field */
        public byte from_ecu_address;                       /* From which ECU came this message */
    };

    /* PGN: 0x00FEEB - Storing the component identification from the reading process */

    public class ComponentIdentification
    {
        public byte length_of_each_field { get; set; }                   /* The real length of the fields - Not part of J1939 standard, only for the user  */
        public byte[] component_product_date { get; set; } = new byte[Constant.MAX_IDENTIFICATION]; /* ASCII field */
        public byte[] component_model_name { get; set; } = new byte[Constant.MAX_IDENTIFICATION];   /* ASCII field */
        public byte[] component_serial_number { get; set; } = new byte[Constant.MAX_IDENTIFICATION];/* ASCII field */
        public byte[] component_unit_name { get; set; } = new byte[Constant.MAX_IDENTIFICATION];    /* ASCII field */
        public byte from_ecu_address { get; set; }                      /* From which ECU came this message */
    };

    /* PGN: 0x00EE00 - Storing the Address claimed from the reading process */

    public class Name
    {
        public uint identity_number { get; set; }                       /* Specify the ECU serial ID - 0 to 2097151 */
        public ushort manufacturer_code { get; set; }                     /* Specify the ECU manufacturer code - 0 to 2047 */
        public byte function_instance { get; set; }                      /* Specify the ECU function number - 0 to 31 */
        public byte ECU_instance { get; set; }                           /* Specify the ECU number - 0 to 7 */
        public byte function { get; set; }                               /* Specify the ECU function - 0 to 255 */
        public byte vehicle_system { get; set; }                         /* Specify the type of vehicle where ECU is located - 0 to 127 */
        public byte arbitrary_address_capable { get; set; }              /* Specify if the ECU have right to change address if addresses conflicts - 0 to 1 */
        public byte industry_group { get; set; }                         /* Specify the group where this ECU is located - 0 to 7 */
        public byte vehicle_system_instance { get; set; }                /* Specify the vehicle system number - 0 to 15 */
        public byte from_ecu_address { get; set; }                       /* From which ECU came this message */
    };

    public class Identifications
    {
        public SoftwareIdentification SoftwareIdentification { get; set; } = new SoftwareIdentification();
        public ECUIdentification EcuIdentification { get; set; } = new ECUIdentification();
        public ComponentIdentification ComponentIdentification { get; set; } = new ComponentIdentification();
    }

    public class SenderInfo
    {
        public Name Name { get; set; } = new Name();
        public byte Address { get; set; }
        public Identifications Identifications { get; set; } = new Identifications();
    }

    /// <summary>
    /// PGN: 0x00EC00 - Storing the Transport Protocol Connection Management from the reading process
    /// </summary>
    public class TP_CM
    {
        /* General */
        public byte ControlByte { get; set; } // What type of message are we going to send

        /* RTS */
        public ushort TotalMessageSizeBeingTransmitted { get; set; } // Total bytes our complete message includes - 9 to 1785
        public byte NumberOfPackagesBeingTransmitted { get; set; } // How many times we are going to send packages via TP_DT - 2 to 224

        /* CTS */
        public byte TotalNumberOfPackagesTransmitted { get; set; } // Total packages we have received
        public byte NextPacketNumberTransmitted { get; set; } // Next packet number we want to have from the transmitter

        /* EOM */
        public ushort TotalNumberOfBytesReceived { get; set; } // Total bytes we are have got
        public byte TotalNumberOfPackagesReceived { get; set; } // Total number of packages received

        /* Abort */
        public byte ConnectingAbortReason { get; set; } // Given a message about the reason

        /* General */
        public uint PGNOfThePacketedMessage { get; set; } // Our message is going to activate a PGN
        public byte FromEcuAddress { get; set; } // From which ECU came this message
    }

    /// <summary>
    /// Control bytes enums
    /// </summary>
    public enum ControlByteCodes : byte
    {
        TP_CM_ABORT = 0xFF,
        TP_CM_BAM = 0x20,
        TP_CM_EndOfMsgACK = 0x13,
        TP_CM_CTS = 0x11,
        TP_CM_RTS = 0x10,
        ACKNOWLEDGEMENT_PGN_SUPPORTED = 0x0,
        ACKNOWLEDGEMENT_PGN_NOT_SUPPORTED = 0x1,
        ACKNOWLEDGEMENT_PGN_ACCESS_DENIED = 0x2,
        ACKNOWLEDGEMENT_PGN_BUSY = 0x3
        // Add more control bytes here
    }

    /// <summary>
    /// PGN: 0x00ED00 - Storing the Transport Protocol Data Transfer from the reading process
    /// </summary>
    public class TP_DT
    {
        public byte SequenceNumber { get; set; } // When this sequence number is the same as number_of_packages from TP_CM, then we have our complete message
        public byte[] Data { get; set; } = new byte[Constant.MAX_TP_DT]; // This is the collected data we are going to send. Also we are using this as a filler
        public byte FromEcuAddress { get; set; } // From which ECU came this message
    }

    ///<summary>
    ///This struct is used for handling J1939 information
    ///</summary>
    public class Link
    {
        /// <summary>
        /// For ID information about this ECU - SAE J1939
        /// </summary>
        public SenderInfo SendInfo { get; set; } = new SenderInfo();
        public TP_DT ReceiveTP_DT { get; set; } = new TP_DT();
        public TP_CM ReceiveTP_CM { get; set; } = new TP_CM();
        public TP_DT SendTP_DT { get; set; } = new TP_DT();
        public TP_CM SendTP_CM { get; set; } = new TP_CM();
    }
    /* Enums for the acknowledgements */
    public enum GroupFunctionValueCodes : byte
    {
        NORMAL = 0x0,
        CANNOT_MAINTAIN_ANOTHER_CONNECTION = 0x1,
        LACKING_NECESSARY_RESOURCES = 0x2,
        ABORT_TIME_OUT = 0x3,
        NO_CAUSE = 0xFF
    }
    public enum J1939Error
    {
        OK,
    }
}
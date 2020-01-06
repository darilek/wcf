using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.Transactions.Wsat.Protocol;

namespace System.ServiceModel.Transactions
{
    internal class WhereaboutsReader
    {
        private static Guid GuidWhereaboutsInfo = new Guid("{2adb4462-bd41-11d0-b12e-00c04fc2f3ef}");
        private string hostName;
        private ProtocolInformationReader protocolInfo;
        private const long STmToTmProtocolSize = 8;

        public WhereaboutsReader(byte[] whereabouts)
        {
            this.DeserializeWhereabouts(new MemoryStream(whereabouts, 0, whereabouts.Length, false, true));
        }

        public string HostName
        {
            get
            {
                return this.hostName;
            }
        }

        public ProtocolInformationReader ProtocolInformation
        {
            get
            {
                return this.protocolInfo;
            }
        }

        private void DeserializeWhereabouts(MemoryStream mem)
        {
            if (SerializationUtils.ReadGuid(mem) != WhereaboutsReader.GuidWhereaboutsInfo)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new SerializationException(System.ServiceModel.SR.GetString("WhereaboutsSignatureMissing")));
            uint num = SerializationUtils.ReadUInt(mem);
            if ((long)num * 8L > mem.Length - mem.Position)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new SerializationException(System.ServiceModel.SR.GetString("WhereaboutsImplausibleProtocolCount")));
            for (uint index = 0; index < num; ++index)
                this.DeserializeWhereaboutsProtocol(mem);
            if (string.IsNullOrEmpty(this.hostName))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new SerializationException(System.ServiceModel.SR.GetString("WhereaboutsNoHostName")));
        }

        private void DeserializeWhereaboutsProtocol(MemoryStream mem)
        {
            WhereaboutsReader.TmProtocol tmProtocol = (WhereaboutsReader.TmProtocol)SerializationUtils.ReadInt(mem);
            uint cbTmProtocolData = SerializationUtils.ReadUInt(mem);
            switch (tmProtocol)
            {
                case WhereaboutsReader.TmProtocol.TmProtocolMsdtcV2:
                    this.ReadMsdtcV2Protocol(mem, cbTmProtocolData);
                    break;
                case WhereaboutsReader.TmProtocol.TmProtocolExtended:
                    this.ReadExtendedProtocol(mem, cbTmProtocolData);
                    break;
                default:
                    SerializationUtils.IncrementPosition(mem, (long)cbTmProtocolData);
                    break;
            }
            SerializationUtils.AlignPosition(mem, 4);
        }

        private void ReadMsdtcV2Protocol(MemoryStream mem, uint cbTmProtocolData)
        {
            if (cbTmProtocolData > 32U)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new SerializationException(System.ServiceModel.SR.GetString("WhereaboutsImplausibleHostNameByteCount")));
            byte[] bytes = SerializationUtils.ReadBytes(mem, (int)cbTmProtocolData);
            int count = 0;
            while ((long)count < (long)(cbTmProtocolData - 1U) && (bytes[count] != (byte)0 || bytes[count + 1] != (byte)0))
                count += 2;
            if (count == 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new SerializationException(System.ServiceModel.SR.GetString("WhereaboutsInvalidHostName")));
            try
            {
                this.hostName = Encoding.Unicode.GetString(bytes, 0, count);
            }
            catch (ArgumentException ex)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new SerializationException(System.ServiceModel.SR.GetString("WhereaboutsInvalidHostName"), (Exception)ex));
            }
        }

        private void ReadExtendedProtocol(MemoryStream mem, uint cbTmProtocolData)
        {
            Guid guid = SerializationUtils.ReadGuid(mem);
            if (guid == PluggableProtocol10.ProtocolGuid || guid == PluggableProtocol11.ProtocolGuid)
                this.protocolInfo = new ProtocolInformationReader(mem);
            else
                SerializationUtils.IncrementPosition(mem, (long)(cbTmProtocolData - 16U));
        }

        private enum TmProtocol
        {
            TmProtocolNone,
            TmProtocolTip,
            TmProtocolMsdtcV1,
            TmProtocolMsdtcV2,
            TmProtocolExtended,
        }
    }
}
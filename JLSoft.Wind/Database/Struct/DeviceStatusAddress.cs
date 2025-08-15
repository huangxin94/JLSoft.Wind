using System;

namespace JLSoft.Wind.Database.Struct
{
    /// <summary>
    /// ��ʾһ���豸��״̬��ַӳ�䣨��S1��4��״̬��ַ����
    /// </summary>
    public class DeviceStatusAddress
    {
        /// <summary>
        /// �豸��ţ���"S1"��"S2"�ȣ�
        /// </summary>
        public string DeviceCode { get; set; }

        /// <summary>
        /// ���豸��Ӧ��4������PLC��ַ����0x0EC4, 0x0EC5, 0x0EC6, 0x0EC7��
        /// </summary>
        public int[] PlcAddresses { get; set; }
    }
}
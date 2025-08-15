using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLSoft.Wind.Class.Models;
using JLSoft.Wind.Logs;

namespace JLSoft.Wind.Services.Connect
{
    public class JobDataReader
    {
        private readonly PlcConnection _plcConnection;
        private readonly DeviceJobDataConfig _config;

        public JobDataReader(PlcConnection plcConnection, DeviceJobDataConfig config)
        {
            _plcConnection = plcConnection;
            _config = config;
        }

        public async Task<JobDataRecord> ReadJobDataAsync()
        {
            // 读取全部80个字（总长度）
            var result = await _plcConnection.ReadDataAsync(
                $"W{_config.JobDataStartAddress:X5}",
                80);

            if (!result.IsSuccess || result.Content.Length < 80)
            {
                throw new Exception($"设备{_config.DeviceCode} Job Data读取失败");
            }

            var jobData = new JobDataRecord();
            int offset = 0;

            // 解析JOB ID (10字节=5个字)
            jobData.JOB_ID = ParseAscii(result.Content, offset, 5);
            offset += 5;

            // 解析JOBID_Pair (10字节=5个字)
            jobData.JOBID_Pair = ParseAscii(result.Content, offset, 5);
            offset += 5;

            // 解析Version Number (1个字)
            jobData.VersionNumber = result.Content[offset++];

            // 解析Type (2个字=4字节)
            jobData.Type = BitConverter.ToInt32(new byte[] {
            (byte)(result.Content[offset] & 0xFF),
            (byte)(result.Content[offset] >> 8),
            (byte)(result.Content[offset + 1] & 0xFF),
            (byte)(result.Content[offset + 1] >> 8)
            }, 0);
            offset += 2;

            // 解析Style (1个字)
            jobData.Style = result.Content[offset++];

            // 解析ProcessFlag (2个字=4字节=32位)
            jobData.ProcessFlag = ParseProcessFlag(result.Content[offset], result.Content[offset + 1]);
            offset += 2;

            // 解析PPID (1个字)
            jobData.PPID = result.Content[offset];

            return jobData;
        }

        public async Task WriteJobDataAsync(JobDataRecord jobData)
        {
            // 构建short数组，长度80（与读取一致）
            short[] data = new short[80];
            int offset = 0;

            // JOB_ID (10字节=5个字)
            WriteAscii(jobData.JOB_ID, data, offset, 5);
            offset += 5;

            // JOBID_Pair (10字节=5个字)
            WriteAscii(jobData.JOBID_Pair, data, offset, 5);
            offset += 5;

            // Version Number (1个字)
            data[offset++] = (short)jobData.VersionNumber;

            // Type (2个字=4字节)
            var typeBytes = BitConverter.GetBytes(jobData.Type);
            data[offset] = (short)(typeBytes[0] | (typeBytes[1] << 8));
            data[offset + 1] = (short)(typeBytes[2] | (typeBytes[3] << 8));
            offset += 2;

            // Style (1个字)
            data[offset++] = (short)jobData.Style;

            // ProcessFlag (2个字)
            var (word1, word2) = BuildProcessFlag(jobData.ProcessFlag);
            data[offset++] = word1;
            data[offset++] = word2;

            // PPID (1个字)
            data[offset] = (short)jobData.PPID;

            // 写入PLC
            var result = await _plcConnection.Plc.WriteDataAsync(
                $"W{_config.JobDataStartAddress:X5}",
                data);

            if (!result.IsSuccess)
            {
                LogManager.Log($"设备{_config.DeviceCode} Job Data写入失败: {result.Message}");
            }
        }

        private string ParseAscii(short[] data, int offset, int wordCount)
        {
            byte[] bytes = new byte[wordCount * 2];

            for (int i = 0; i < wordCount; i++)
            {
                bytes[i * 2] = (byte)(data[offset + i] & 0xFF);
                bytes[i * 2 + 1] = (byte)((data[offset + i] >> 8) & 0xFF);
            }

            return Encoding.ASCII.GetString(bytes).TrimEnd('\0');
        }

        private JobProcessFlag ParseProcessFlag(short word1, short word2)
        {
            return new JobProcessFlag
            {
                V_Processed = (word1 & 0x01) != 0, // BIT0
                S_Processed = (word1 & 0x02) != 0, // BIT1
                G_Processed = (word1 & 0x04) != 0, // BIT2
                U_Processed = (word1 & 0x08) != 0, // BIT3
                A_Processed = (word1 & 0x10) != 0, // BIT4
                C_Processed = (word1 & 0x20) != 0, // BIT5
                R_Processed = (word1 & 0x40) != 0, // BIT6
                M_Processed = (word1 & 0x80) != 0  // BIT7
            };
        }

        private void WriteAscii(string value, short[] data, int offset, int wordCount)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(value ?? "");
            Array.Resize(ref bytes, wordCount * 2); // 补齐长度
            for (int i = 0; i < wordCount; i++)
            {
                data[offset + i] = (short)(bytes[i * 2] | (bytes[i * 2 + 1] << 8));
            }
        }

        private (short, short) BuildProcessFlag(JobProcessFlag flag)
        {
            short word1 = 0;
            if (flag.V_Processed) word1 |= 0x01;
            if (flag.S_Processed) word1 |= 0x02;
            if (flag.G_Processed) word1 |= 0x04;
            if (flag.U_Processed) word1 |= 0x08;
            if (flag.A_Processed) word1 |= 0x10;
            if (flag.C_Processed) word1 |= 0x20;
            if (flag.R_Processed) word1 |= 0x40;
            if (flag.M_Processed) word1 |= 0x80;
            short word2 = 0; // 目前未用到
            return (word1, word2);
        }
    }
}

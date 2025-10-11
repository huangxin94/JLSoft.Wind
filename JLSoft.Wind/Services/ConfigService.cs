using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using JLSoft.Wind.Database;
using JLSoft.Wind.Database.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace JLSoft.Wind.Services
{
    public class ConfigService
    {
        private const string JsonFilePath = "BasicConfiguration.json";

        // 单例模式确保全局只有一个配置服务实例
        private static readonly Lazy<ConfigService> _instance =
            new Lazy<ConfigService>(() => new ConfigService());
        public static ConfigService Instance => _instance.Value;

        private BasicConfiguration _config;

        private ConfigService()
        {
            LoadConfiguration();
        }

        // 加载或创建配置文件
        public void LoadConfiguration()
        {
            try
            {
                if (File.Exists(JsonFilePath))
                {
                    string json = File.ReadAllText(JsonFilePath);
                    _config = JsonConvert.DeserializeObject<BasicConfiguration>(json);
                }
                else
                {
                    // 创建默认配置
                    _config = new BasicConfiguration
                    {
                        Robot = new RobotConfig
                        {
                            IpAddress = "192.168.29.130",
                            Port = 8001
                        },
                        Plc = new PLCConfig
                        {
                            IpAddress = "127.0.0.1",
                            Port = 8002
                        },
                        AngleTConfig = new AngleTConfig
                        {
                            Ip = "192.168.1.3",
                            Port = 8080,
                            Angle = "0"
                        },
                        AlignerConfig = new AlignerConfig
                        {
                            ComPort = "COM3",
                            GLM = "1",
                            FWO = "1",
                            WT = "1"
                        },
                        LoadPort1Config = "COM1",
                        LoadPort2Config = "COM2",
                        AlignerOCRConfig = new PLCConfig
                        {
                            IpAddress = "192.168.1.11",
                            Port = 8500
                        },
                        AngleTOCRConfig = new PLCConfig
                        {
                            IpAddress = "192.168.1.10",
                            Port = 8500

                        },
                        TeachServerConfig = new PLCConfig
                        {
                            IpAddress = "192.168.1.252",
                            Port = 8600
                        },
                        Users = new List<UserData>()
                    };
                    SaveConfiguration();
                }
            }
            catch (Exception ex)
            {
                // 处理加载错误
                Console.WriteLine($"加载配置文件失败: {ex.Message}");
                _config = CreateDefaultConfig();
            }
        }

        // 保存配置
        public void SaveConfiguration()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_config, Formatting.Indented);
                File.WriteAllText(JsonFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存配置文件失败: {ex.Message}");
            }
        }

        // 机器人配置相关方法
        public static string GetRobotIp() => Instance._config.Robot.IpAddress;
        public static int GetRobotPort() => Instance._config.Robot.Port;

        public static string GetPlcIp() => Instance._config.Plc.IpAddress;
        public static int GetPlcPort() => Instance._config.Plc.Port;

        public static string GetAngleTIp() => Instance._config.AngleTConfig.Ip;

        public static int GetAngleTPort() => Instance._config.AngleTConfig.Port;

        public static string GetAngleTAngle() => Instance._config.AngleTConfig.Angle;

        public static string GetAlignerComPort() => Instance._config.AlignerConfig.ComPort;
        public static string GetAlignerGLM() => Instance._config.AlignerConfig.GLM;
        public static string GetAlignerWT() => Instance._config.AlignerConfig.WT;
        public static string GetAlignerFWO() => Instance._config.AlignerConfig.FWO;
        public static string GetLoadPort1ComPort() => Instance._config.LoadPort1Config;
        public static string GetLoadPort2ComPort() => Instance._config.LoadPort2Config;
        public static string GetAlignerOCRIp() => Instance._config.AlignerOCRConfig.IpAddress;
        public static int GetAlignerOCRPort() => Instance._config.AlignerOCRConfig.Port;
        public static string GetAngleTOCRIp() => Instance._config.AngleTOCRConfig.IpAddress;
        public static int GetAngleTOCRPort() => Instance._config.AngleTOCRConfig.Port;

        public static string GetTeachServerIp() => Instance._config.TeachServerConfig.IpAddress;
        public static int GetTeachServerPort() => Instance._config.TeachServerConfig.Port;
        // 用户管理相关方法
        public List<UserData> GetUsers() => _config.Users;
        public void AddUser(UserData user)
        {
            _config.Users.Add(user);
            SaveConfiguration();
        }
        public void UpdateUsers(List<UserData> users)
        {
            _config.Users = users;
            SaveConfiguration();
        }

        // 获取设备编号到索引的映射
        public static List<DeviceIndices> GetDeviceStations()
        {
            return Instance._config.Site;
        }
        public static bool IsPlcConnectionEnabled()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false , reloadOnChange: true) 
                .Build();
            return configuration.GetValue<bool>("AppSetting:EnablePlcConnection");
        }
        // 获取统一的位置列表（包含主站和子站）
        public static List<PositionInfo> GetDevicePositions(string deviceCode)
        {
            var station = GetDeviceStations();
            var positionList = new List<PositionInfo>();
            foreach (var item in station)
            {
                if (item.SubStations.Count() > 0)
                {
                    foreach (var subStation in item.SubStations)
                    {
                        PositionInfo info = new PositionInfo
                        {
                            name = item.DeviceCode + "_" + subStation.Name,
                            biename = subStation.bieName,
                            positions = subStation.Positions
                        };
                        positionList.Add(info);
                    }

                }
                else
                {
                    PositionInfo info = new PositionInfo
                    {
                        name = item.DeviceCode,
                        biename = item.bieName,
                        positions = item.Positions
                    };
                    positionList.Add(info);
                }
            }

            return positionList;
        }

        public static Positions? FindPosition(string deviceCode, string? subStationName = null)
        {
            var list = GetDevicePositions(deviceCode);
            string key = subStationName == null ? deviceCode : $"{deviceCode}_{subStationName}";
            return list.FirstOrDefault(p => p.name == key)?.positions;
        }
        public static string? FindbieName(string deviceCode, string? subStationName = null)
        {
            var list = GetDevicePositions(deviceCode);
            string key = subStationName == null ? deviceCode : $"{deviceCode}_{subStationName}";
            return list.FirstOrDefault(p => p.name == key)?.biename;
        }
        // 根据设备编号获取索引（如无则抛异常或返回-1）
        public static int GetDeviceIndex(string deviceCode)
        {
            try
            {
                var dict = GetDeviceStations();
                return dict.Where(x => x.DeviceCode == deviceCode).FirstOrDefault().Index;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取设备索引失败: {ex.Message}");
                return -1; // 或抛出异常 

            }
        }

        public static string GetDeviceName(int deviceindex)
        {
            try
            {
                var dict = GetDeviceStations();
                return dict.Where(x => x.Index == deviceindex).FirstOrDefault().DeviceCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取设备索引失败: {ex.Message}");
                return ""; // 或抛出异常 

            }
        }

        // 创建默认配置
        private BasicConfiguration CreateDefaultConfig()
        {
            return new BasicConfiguration
            {
                Robot = new RobotConfig
                {
                    IpAddress = "127.0.0.1",
                    Port = 8000
                },
                Users = new List<UserData>()
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLSoft.Wind.Database.Models;
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
                            IpAddress = "127.0.0.1",
                            Port = 8001
                        },
                        Plc = new PLCConfig
                        {
                            IpAddress = "127.0.0.1",
                            Port = 8002
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

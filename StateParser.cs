using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using tfstate_report.Data;
using tfstate_report.Models;

namespace tfstate_report
{
    public class StateParser
    {
        private readonly string _stateFile;
        private readonly StateReader _stateReader;

        public StateParser(string stateFile)
        {
            _stateFile = stateFile;
            _stateReader = new StateReader();
        }

        public RoleStats[] ParseState()
        {
            JObject stateData = _stateReader.GetStateData(_stateFile);
            JArray modules = (JArray)stateData["modules"];
            List<RoleStats> roleStatsList = new List<RoleStats>();

            foreach (var module in modules)
            {
                RoleStats roleStats = GetRoleStats(module);

                if (roleStats != null)
                {
                    var resources = (JObject)module["resources"];
                    List<VMStats> vmStatsList = new List<VMStats>();

                    foreach (var resource in resources.Properties())
                    {
                        if (resource.Name.StartsWith("azurerm_virtual_machine."))
                        {
                            var attributes = (JObject)resource.Value["primary"]["attributes"];
                            VMStats vmStats = GetVMStats(attributes);
                            vmStats.Disks = GetDiskStats(attributes);
                            vmStatsList.Add(vmStats);
                        }
                    }

                    roleStats.VMs = vmStatsList.ToArray();
                    roleStatsList.Add(roleStats);
                }
            }

            return roleStatsList.ToArray();
        }

        private RoleStats GetRoleStats(JToken module)
        {
            JArray path = (JArray)module["path"];

            if (path.Count > 1)
            {
                string roleName = (string)path[1];
                var roleStats = new RoleStats();
                roleStats.Name = roleName;
                return roleStats;
            }

            return null;
        }

        private VMStats GetVMStats(JObject attributes)
        {
            var vmStats = new VMStats();

            vmStats.Size = (string)attributes["vm_size"];

            var osInfo = attributes.Properties()
                .Where(attrib => attrib.Name.StartsWith("storage_image_reference."));
            string osPublisher = (string)osInfo.First(prop => prop.Name.EndsWith(".publisher")).Value;
            string osOffer = (string)osInfo.First(prop => prop.Name.EndsWith(".offer")).Value;
            string osSku = (string)osInfo.First(prop => prop.Name.EndsWith(".sku")).Value;
            string osVersion = (string)osInfo.First(prop => prop.Name.EndsWith(".version")).Value;
            vmStats.OS = string.Format("{0}:{1}:{2}:{3}", osPublisher, osOffer, osSku, osVersion);

            return vmStats;
        }

        private DiskStats[] GetDiskStats(JObject attributes)
        {
            var diskStatsList = new List<DiskStats>();
            int dataDiskCount = Convert.ToInt32(attributes["storage_data_disk.#"].ToString());

            for (int i = 0; i < dataDiskCount; i++)
            {
                DiskStats diskStats = new DiskStats();
                diskStats.Size = Convert.ToInt32(attributes[string.Format("storage_data_disk.{0}.disk_size_gb", i)].ToString());
                diskStats.Type = (string)attributes[string.Format("storage_data_disk.{0}.managed_disk_type", i)];
                diskStatsList.Add(diskStats);
            }

            var osDiskInfo = attributes.Properties()
                .Where(attrib => attrib.Name.StartsWith("storage_os_disk."));
            DiskStats osDiskStats = new DiskStats();
            osDiskStats.Size = Convert.ToInt32(osDiskInfo.First(prop => prop.Name.EndsWith(".disk_size_gb")).Value.ToString());
            osDiskStats.Type = (string)osDiskInfo.First(prop => prop.Name.EndsWith(".managed_disk_type")).Value;
            diskStatsList.Add(osDiskStats);

            return diskStatsList.ToArray();
        }
    }
}
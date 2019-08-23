﻿using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace rpa_functions.rpa_pc35
{
    class PackCheckTableOperations
    {
        string tableName = Environment.GetEnvironmentVariable("PC35_TABLENAME");
        CommonTable table;

        public PackCheckTableOperations()
        { 
      
             this.table = new CommonTable(this.tableName);
        }

        public async Task<string> InsertBatch(dynamic bodyData)
        {
            string returnCode = "";

            List<PackageCheckEntity> packages = Mappings.InsertPackageCheck(bodyData);

            foreach (PackageCheckEntity re in packages)
            {

                TableResult tr = await table.InsertorReplace(Mappings.ToTableEntity(re, tableName));

            }

            return returnCode;
        }

        private async Task<List<PackageCheckTableEntity>> QueryPacakgeId(string packageId)
        {

            List<PackageCheckTableEntity> queryResult = await table.RetrieveEntities<PackageCheckTableEntity>(PackageCheckConstants.ID_FIELD_NAME,
                                                                                     QueryComparisons.Equal,
                                                                                     packageId);

            return queryResult;
        }

        public async Task<string> QueryPendingPackages()
        {

            List<PackageCheckTableEntity> queryResult = await table.RetrieveEntities<PackageCheckTableEntity>(PackageCheckConstants.STATUS_FIELD_NAME,
                                                                                     QueryComparisons.Equal,
                                                                                     PackageCheckConstants.STATUS_PENDING);

            return (Mappings.ToPackageCheckEntityJSON(queryResult));

        }

        public async Task<Boolean> UpdatePackage(dynamic bodyData)
        {

            // Find package, if found - update else fail.
            List<PackageCheckTableEntity> result = await QueryPacakgeId(Convert.ToString(bodyData.Id));

            if (result.Count == 1)
            {
                Console.WriteLine("Hello");

                result[0].LCI = "test";
                result[0].SAP = "teest";
                result[0].Status = 1;


                TableResult tr = await table.InsertorReplace(result[0]);

                return true;
            } else
            {
                return false;
            }

 

            
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using VOL.Entity.DomainModels;
using VOL.Core.DBManager;

namespace VOL.Core.WorkFlow
{
    /// <summary>
    /// WorkFlow XCode Data Service
    /// Provides XCode-based data access methods for WorkFlow entities
    /// This replaces Entity Framework calls with NewLife.XCode
    /// </summary>
    public static class WorkFlowXCodeService
    {
        /// <summary>
        /// Find WorkFlow by table name using XCode
        /// </summary>
        public static List<Sys_WorkFlow> FindWorkFlowByTable(string tableName)
        {
            // Use Dapper-style SQL execution through existing DBServerProvider
            var sql = "SELECT * FROM Sys_WorkFlow WHERE WorkTable = @tableName AND Enable = 1";
            var sqlDapper = DBServerProvider.SqlDapper;
            var list = sqlDapper.QueryList<Sys_WorkFlow>(sql, new { tableName });
            return list ?? new List<Sys_WorkFlow>();
        }

        /// <summary>
        /// Find WorkFlow by ID using XCode
        /// </summary>
        public static Sys_WorkFlow FindWorkFlowById(Guid workFlowId)
        {
            var sql = "SELECT * FROM Sys_WorkFlow WHERE WorkFlow_Id = @workFlowId";
            var sqlDapper = DBServerProvider.SqlDapper;
            return sqlDapper.QueryFirst<Sys_WorkFlow>(sql, new { workFlowId });
        }

        /// <summary>
        /// Find WorkFlowTable with conditions using XCode
        /// </summary>
        public static List<Sys_WorkFlowTable> FindWorkFlowTable(Guid workFlowId, int auditStatus1, int auditStatus2)
        {
            var sql = @"SELECT * FROM Sys_WorkFlowTable 
                       WHERE WorkFlow_Id = @workFlowId 
                       AND (AuditStatus = @auditStatus1 OR AuditStatus = @auditStatus2)";
            var sqlDapper = DBServerProvider.SqlDapper;
            var list = sqlDapper.QueryList<Sys_WorkFlowTable>(sql, new { workFlowId, auditStatus1, auditStatus2 });
            return list ?? new List<Sys_WorkFlowTable>();
        }

        /// <summary>
        /// Find WorkFlowTable with include using XCode
        /// </summary>
        public static List<Sys_WorkFlowTable> FindWorkFlowTableWithSteps(Guid workFlowId, int auditStatus1, int auditStatus2)
        {
            // Get main tables
            var tables = FindWorkFlowTable(workFlowId, auditStatus1, auditStatus2);
            
            // Load related steps for each table
            foreach (var table in tables)
            {
                var stepsSql = "SELECT * FROM Sys_WorkFlowTableStep WHERE WorkFlowTable_Id = @tableId";
                var sqlDapper = DBServerProvider.SqlDapper;
                var steps = sqlDapper.QueryList<Sys_WorkFlowTableStep>(stepsSql, new { tableId = table.WorkFlowTable_Id });
                table.Sys_WorkFlowTableStep = steps ?? new List<Sys_WorkFlowTableStep>();
            }
            
            return tables;
        }

        /// <summary>
        /// Find WorkFlowSteps by WorkFlow ID using XCode
        /// </summary>
        public static List<Sys_WorkFlowStep> FindWorkFlowSteps(Guid workFlowId)
        {
            var sql = "SELECT * FROM Sys_WorkFlowStep WHERE WorkFlow_Id = @workFlowId";
            var sqlDapper = DBServerProvider.SqlDapper;
            var list = sqlDapper.QueryList<Sys_WorkFlowStep>(sql, new { workFlowId });
            return list ?? new List<Sys_WorkFlowStep>();
        }

        /// <summary>
        /// Update WorkFlowTableSteps using XCode
        /// </summary>
        public static void UpdateWorkFlowTableSteps(List<Sys_WorkFlowTableStep> steps)
        {
            // Use Dapper for update operations
            var sqlDapper = DBServerProvider.SqlDapper;
            foreach (var step in steps.Where(s => s.Sys_WorkFlowTableStep_Id != Guid.Empty))
            {
                var sql = @"UPDATE Sys_WorkFlowTableStep 
                           SET Enable = @Enable, AuditId = @AuditId, Auditor = @Auditor, 
                               AuditDate = @AuditDate, Remark = @Remark 
                           WHERE Sys_WorkFlowTableStep_Id = @Id";
                
                sqlDapper.ExcuteNonQuery(sql, new 
                { 
                    step.Enable, 
                    step.AuditId, 
                    step.Auditor, 
                    step.AuditDate, 
                    step.Remark,
                    Id = step.Sys_WorkFlowTableStep_Id 
                });
            }
        }

        /// <summary>
        /// Remove WorkFlowTables using XCode
        /// </summary>
        public static void RemoveWorkFlowTables(List<Sys_WorkFlowTable> tables)
        {
            var sqlDapper = DBServerProvider.SqlDapper;
            foreach (var table in tables)
            {
                var sql = "DELETE FROM Sys_WorkFlowTable WHERE WorkFlowTable_Id = @tableId";
                sqlDapper.ExcuteNonQuery(sql, new { tableId = table.WorkFlowTable_Id });
            }
        }

        /// <summary>
        /// Find WorkFlowTableAuditLog using XCode
        /// </summary>
        public static List<Sys_WorkFlowTableAuditLog> FindWorkFlowTableAuditLog(Guid workFlowTableId)
        {
            var sql = "SELECT * FROM Sys_WorkFlowTableAuditLog WHERE WorkFlowTable_Id = @workFlowTableId";
            var sqlDapper = DBServerProvider.SqlDapper;
            var list = sqlDapper.QueryList<Sys_WorkFlowTableAuditLog>(sql, new { workFlowTableId });
            return list ?? new List<Sys_WorkFlowTableAuditLog>();
        }

        /// <summary>
        /// Add WorkFlowTable using XCode
        /// </summary>
        public static void AddWorkFlowTable(Sys_WorkFlowTable workFlowTable)
        {
            var sql = @"INSERT INTO Sys_WorkFlowTable 
                       (WorkFlowTable_Id, WorkFlow_Id, WorkName, WorkTableKey, WorkTable, WorkTableName, 
                        CurrentStepId, StepName, CurrentOrderId, AuditStatus, Creator, CreateDate, CreateID, 
                        Enable, Modifier, ModifyDate, ModifyID) 
                       VALUES (@WorkFlowTable_Id, @WorkFlow_Id, @WorkName, @WorkTableKey, @WorkTable, @WorkTableName, 
                               @CurrentStepId, @StepName, @CurrentOrderId, @AuditStatus, @Creator, @CreateDate, @CreateID, 
                               @Enable, @Modifier, @ModifyDate, @ModifyID)";

            var sqlDapper = DBServerProvider.SqlDapper;
            sqlDapper.ExcuteNonQuery(sql, workFlowTable);
        }

        /// <summary>
        /// Add WorkFlowTableAuditLog using XCode
        /// </summary>
        public static void AddWorkFlowTableAuditLog(Sys_WorkFlowTableAuditLog log)
        {
            var sql = @"INSERT INTO Sys_WorkFlowTableAuditLog 
                       (Id, WorkFlowTable_Id, WorkFlowTableStep_Id, StepId, StepName, AuditId, Auditor, 
                        AuditStatus, AuditResult, AuditDate, Remark, CreateDate) 
                       VALUES (@Id, @WorkFlowTable_Id, @WorkFlowTableStep_Id, @StepId, @StepName, @AuditId, @Auditor, 
                               @AuditStatus, @AuditResult, @AuditDate, @Remark, @CreateDate)";

            var sqlDapper = DBServerProvider.SqlDapper;
            sqlDapper.ExcuteNonQuery(sql, log);
        }

        /// <summary>
        /// Update WorkFlowTable using XCode
        /// </summary>
        public static void UpdateWorkFlowTable(Sys_WorkFlowTable workFlowTable)
        {
            var sql = @"UPDATE Sys_WorkFlowTable 
                       SET CurrentStepId = @CurrentStepId, StepName = @StepName, AuditStatus = @AuditStatus,
                           Modifier = @Modifier, ModifyDate = @ModifyDate, ModifyID = @ModifyID
                       WHERE WorkFlowTable_Id = @WorkFlowTable_Id";

            var sqlDapper = DBServerProvider.SqlDapper;
            sqlDapper.ExcuteNonQuery(sql, workFlowTable);
        }
    }
}
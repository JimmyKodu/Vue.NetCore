using System;
using System.Collections.Generic;
using System.Linq;
using VOL.Entity.DomainModels;
using XCode;
using XCode.DataAccessLayer;

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
            // Use XCode DAL with raw SQL for WorkFlow queries
            var sql = "SELECT * FROM Sys_WorkFlow WHERE WorkTable = @tableName AND Enable = 1";
            var dal = DAL.Create("default");
            var dt = dal.Query(sql, new { tableName });
            
            // Convert DataTable to entity list (simplified approach)
            var list = new List<Sys_WorkFlow>();
            // TODO: Implement DataTable to entity conversion
            return list;
        }

        /// <summary>
        /// Find WorkFlow by ID using XCode
        /// </summary>
        public static Sys_WorkFlow FindWorkFlowById(Guid workFlowId)
        {
            var sql = "SELECT * FROM Sys_WorkFlow WHERE WorkFlow_Id = @workFlowId";
            return Entity.Query<Sys_WorkFlow>(sql, new { workFlowId })?.FirstOrDefault();
        }

        /// <summary>
        /// Find WorkFlowTable with conditions using XCode
        /// </summary>
        public static List<Sys_WorkFlowTable> FindWorkFlowTable(Guid workFlowId, int auditStatus1, int auditStatus2)
        {
            var sql = @"SELECT * FROM Sys_WorkFlowTable 
                       WHERE WorkFlow_Id = @workFlowId 
                       AND (AuditStatus = @auditStatus1 OR AuditStatus = @auditStatus2)";
            var list = Entity.Query<Sys_WorkFlowTable>(sql, new { workFlowId, auditStatus1, auditStatus2 });
            return list?.ToList() ?? new List<Sys_WorkFlowTable>();
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
                var steps = Entity.Query<Sys_WorkFlowTableStep>(stepsSql, new { tableId = table.WorkFlowTable_Id });
                table.Sys_WorkFlowTableStep = steps?.ToList() ?? new List<Sys_WorkFlowTableStep>();
            }
            
            return tables;
        }

        /// <summary>
        /// Find WorkFlowSteps by WorkFlow ID using XCode
        /// </summary>
        public static List<Sys_WorkFlowStep> FindWorkFlowSteps(Guid workFlowId)
        {
            var sql = "SELECT * FROM Sys_WorkFlowStep WHERE WorkFlow_Id = @workFlowId";
            var list = Entity.Query<Sys_WorkFlowStep>(sql, new { workFlowId });
            return list?.ToList() ?? new List<Sys_WorkFlowStep>();
        }

        /// <summary>
        /// Update WorkFlowTableSteps using XCode
        /// </summary>
        public static void UpdateWorkFlowTableSteps(List<Sys_WorkFlowTableStep> steps)
        {
            // Use XCode's update capabilities
            foreach (var step in steps.Where(s => s.Sys_WorkFlowTableStep_Id != Guid.Empty))
            {
                var sql = @"UPDATE Sys_WorkFlowTableStep 
                           SET Enable = @Enable, AuditId = @AuditId, Auditor = @Auditor, 
                               AuditDate = @AuditDate, Remark = @Remark 
                           WHERE Sys_WorkFlowTableStep_Id = @Id";
                
                Entity.Execute(sql, new 
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
            foreach (var table in tables)
            {
                var sql = "DELETE FROM Sys_WorkFlowTable WHERE WorkFlowTable_Id = @tableId";
                Entity.Execute(sql, new { tableId = table.WorkFlowTable_Id });
            }
        }

        /// <summary>
        /// Find WorkFlowTableAuditLog using XCode
        /// </summary>
        public static List<Sys_WorkFlowTableAuditLog> FindWorkFlowTableAuditLog(Guid workFlowTableId)
        {
            var sql = "SELECT * FROM Sys_WorkFlowTableAuditLog WHERE WorkFlowTable_Id = @workFlowTableId";
            var list = Entity.Query<Sys_WorkFlowTableAuditLog>(sql, new { workFlowTableId });
            return list?.ToList() ?? new List<Sys_WorkFlowTableAuditLog>();
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

            Entity.Execute(sql, workFlowTable);
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

            Entity.Execute(sql, log);
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

            Entity.Execute(sql, workFlowTable);
        }
    }
}
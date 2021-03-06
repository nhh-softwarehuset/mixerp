﻿/********************************************************************************
Copyright (C) Binod Nepal, Mix Open Foundation (http://mixof.org).

This file is part of MixERP.

MixERP is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

MixERP is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with MixERP.  If not, see <http://www.gnu.org/licenses/>.
***********************************************************************************/

using System;
using System.Data;
using MixERP.Net.Common;
using MixERP.Net.Common.Events;
using MixERP.Net.DbFactory;
using Npgsql;

namespace MixERP.Net.Core.Modules.Finance.Data
{
    public class EODOperation
    {
        public event EventHandler<MixERPPGEventArgs> NotificationReceived;

        public static EODStatus GetStatus(int officeId)
        {
            const string sql = "SELECT transactions.get_value_date(@OfficeId::integer) AS value_date, transactions.is_eod_initialized(@OfficeId::integer, transactions.get_value_date(@OfficeId::integer)::date) AS eod_initialized;";
            using (NpgsqlCommand command = new NpgsqlCommand(sql))
            {
                command.Parameters.AddWithValue("@OfficeId", officeId);

                using (DataTable table = DbOperation.GetDataTable(command))
                {
                    if (table.Rows != null && table.Rows.Count.Equals(1))
                    {
                        EODStatus status = new EODStatus();
                        status.ValueDate = Conversion.TryCastDate(table.Rows[0]["value_date"]);
                        status.IsInitialized = Conversion.TryCastBoolean(table.Rows[0]["eod_initialized"]);
                        return status;
                    }
                }
            }

            return null;
        }

        public static DateTime GetValueDate(int officeId)
        {
            const string sql = "SELECT transactions.get_value_date(@OfficeId::integer);";
            using (NpgsqlCommand command = new NpgsqlCommand(sql))
            {
                command.Parameters.AddWithValue("@OfficeId", officeId);

                return Conversion.TryCastDate(DbOperation.GetScalarValue(command));
            }
        }

        public static void Initialize(int userId, int officeId)
        {
            const string sql = "SELECT * FROM transactions.initialize_eod_operation(@UserId::integer, @OfficeId::integer, transactions.get_value_date(@OfficeId::integer)::date);";
            using (NpgsqlCommand command = new NpgsqlCommand(sql))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@OfficeId", officeId);

                DbOperation.ExecuteNonQuery(command);
            }
        }

        public static void Initialize(int userId, int officeId, DateTime valueDate)
        {
            const string sql = "SELECT * FROM transactions.initialize_eod_operation(@UserId::integer, @OfficeId::integer, @ValueDate::date);";
            using (NpgsqlCommand command = new NpgsqlCommand(sql))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@OfficeId", officeId);
                command.Parameters.AddWithValue("@ValueDate", valueDate);

                DbOperation.ExecuteNonQuery(command);
            }
        }

        public void Perform(long loginId)
        {
            const string sql = "SELECT * FROM transactions.perform_eod_operation(@LoginId::bigint);";
            using (NpgsqlCommand command = new NpgsqlCommand(sql))
            {
                command.Parameters.AddWithValue("@LoginId", loginId);
                command.CommandTimeout = 3600;

                DbOperation operation = new DbOperation();
                operation.Listen += this.Listen;
                operation.ListenNonQuery(command);
            }
        }

        private void Listen(object sender, DbNotificationArgs e)
        {
            var notificationReceived = this.NotificationReceived;

            if (notificationReceived != null)
            {
                if (e.Notice == null && !string.IsNullOrWhiteSpace(e.Message))
                {
                    MixERPPGEventArgs args = new MixERPPGEventArgs(e.Message, "error", 0);
                    notificationReceived(this, args);
                    return;
                }

                if (e.Notice != null && e.Notice.Severity.ToUpperInvariant().Equals("INFO"))
                {
                    MixERPPGEventArgs args = new MixERPPGEventArgs(e.Notice.Message, e.Notice.Detail, 0);

                    notificationReceived(this, args);
                }
            }
        }
    }

    public class EODStatus
    {
        public Boolean IsInitialized { get; set; }
        public DateTime ValueDate { get; set; }
    }
}
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
using MixERP.Net.Common.Extensions;
using MixERP.Net.Entities;
using MixERP.Net.Entities.Contracts;
using MixERP.Net.FrontEnd.Base;
using MixERP.Net.FrontEnd.Cache;
using MixERP.Net.i18n.Resources;
using MixERP.Net.WebControls.StockTransactionFactory;

namespace MixERP.Net.Core.Modules.Sales.Entry
{
    public partial class Delivery : MixERPUserControl, ITransaction
    {
        public override void OnControlLoad(object sender, EventArgs e)
        {
            using (StockTransactionForm product = new StockTransactionForm())
            {
                product.Book = TranBook.Sales;
                product.SubBook = SubTranBook.Delivery;
                product.Text = Titles.SalesDelivery;
                product.ShowPaymentTerms = true;
                product.ShowPriceTypes = true;
                product.ShowShippingInformation = true;
                product.ShowSalesAgents = true;
                product.ShowCostCenter = true;
                product.ShowStore = true;
                product.ShowSalesType = true;
                product.VerifyStock = true;
                product.OfficeId = CurrentUser.GetSignInView().OfficeId.ToInt();

                this.Placeholder1.Controls.Add(product);
            }
        }
    }
}
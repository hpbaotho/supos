// SuposDb.cs created with MonoDevelop
// User: xavier at 07:08 6/12/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.IO;

namespace Supos.Core
{
	
	
	public class SuposDb
	{
		// TODO: static or not (GetMedia??)
		private SuposDbProvider provider;
		private SuposDataSet ds;
		static private DbSettings settings;
		
		public SuposDb( DbSettings config )
		{ 
			settings = config;
			ds = new SuposDataSet();
			provider = new SuposDbProvider(config);
		}
		
		public SuposDataSet DataSet
		{
			get { return ds; }
		}
		
		public void Fill()
		{
			provider.Fill(ds);
		}
		
		static public byte[] GetMedia( string media )
		{
			string path = Path.Combine(settings.MediaPath, media);
			if (File.Exists(path) )
			{
				byte[] file = File.ReadAllBytes( path );			 
				return file;
			}
			else return null;
		}
		
		public SuposDataSet.OrdersRow NewOrder()
		{
			SuposDataSet.OrdersRow result = (SuposDataSet.OrdersRow) ds.Orders.NewRow();
			result.Id = Util.GetIdStringNow();
			return result;
		}
		
		public bool AddOrder(SuposDataSet.OrdersRow order)
		{
			ds.Orders.AddOrdersRow(order);		
			return true;
		}
		
		public void SaveOrders()
		{
			this.provider.OrdersAdapter.Update(DataSet);
			this.DataSet.Orders.AcceptChanges();
		}
		
		public SuposDataSet.OrderDetailsRow AddProductInOrder(SuposDataSet.OrdersRow order, SuposDataSet.ProductsRow product)
		{
			if( order==null || product==null)
				return null;
			//TODO: Handle error
			SuposDataSet.OrderDetailsRow row = (SuposDataSet.OrderDetailsRow)ds.OrderDetails.NewRow();
			row.Id = Util.GetIdStringNow();
			row.OrderId = order.Id;
			row.ProductId = product.Id;
			row.TaxId = product.DefaultTaxId;
			row.Quantity = 1;
			row.Price = product.Price;
			ds.OrderDetails.AddOrderDetailsRow(row);
			return row;
		}
		
		public struct OrderTotal
		{
			public System.Decimal TaxAmount;
			public System.Decimal TotPrice;
			
			public System.Decimal TotPriceTaxInc
			{
				get { return Math.Round(TotPrice + TaxAmount, 2); }
			}
			
		}
		
		static public OrderTotal GetOrderTotal( SuposDataSet.OrdersRow order)
		{
			OrderTotal result = new OrderTotal();
			if (order != null)
			{
				SuposDataSet.OrderDetailsRow[] details = (SuposDataSet.OrderDetailsRow[])order.GetChildRows( "FK_orders_OrderDetails" );
				result.TotPrice = 0;
				result.TaxAmount = 0;
				foreach( SuposDataSet.OrderDetailsRow detail in details)
				{
					result.TotPrice += detail.Price*detail.Quantity;
					result.TaxAmount += (Decimal)detail.TaxesRow.Rate * detail.Price;
				}
			}
			return result;
		}
		
	}
}

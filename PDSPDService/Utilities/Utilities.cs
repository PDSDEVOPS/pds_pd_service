using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using Newtonsoft.Json;
using ProductionWebService.Common;

namespace ProductionWebService.Utilities
{
    public class Utility
    {
        public DataTable GetDataTableFromStringArrays(List<string[]> Arrays, string AdditionalParameters, bool HasHeaders = true)
        {
            //const string ADD_PARAMS_TEST = @"{""IntegerFields"":[],""DecimalFields"":[""Oil Prod"",""Gas Prod"",""Gas Sales"",""Water Prod"",""Casing Press"",""Tubing Press""],""DateTimeFields"":[""Date""],""DoubleFields"":[]}";

            var AdditionalParams = JsonConvert.DeserializeObject<AdditionalParameters>(AdditionalParameters);

            Console.Out.WriteLine();

            const int HEADER_ROW_INDEX = 0;
            var CurrentIndex = 0;
            var Table = new DataTable();

            if(HasHeaders)
            {
                var Headers = Arrays[HEADER_ROW_INDEX];

                foreach(var Header in Headers)
                {
                    if (AdditionalParams != null 
                        && AdditionalParams.DateTimeFields != null 
                        && AdditionalParams.DecimalFields != null
                        && AdditionalParams.DoubleFields != null
                        && AdditionalParams.IntegerFields != null)
                    {
                        if (AdditionalParams.DateTimeFields.Any( x => x == Header))
                        {
                            //Table.Columns.Add(Header, typeof(DateTime?));
                            //Nullable.
                            var Col = Table.Columns.Add(Header, typeof(DateTime));
                            Col.AllowDBNull = true;
                        }
                        else if (AdditionalParams.DecimalFields.Any(x => x == Header))
                        {
                            var Col = Table.Columns.Add(Header, typeof(Decimal));
                            Col.AllowDBNull = true;
                        }
                        else if (AdditionalParams.DoubleFields.Any(x => x == Header))
                        {
                            var Col = Table.Columns.Add(Header, typeof(Double));
                            Col.AllowDBNull = true;
                        }
                        else if (AdditionalParams.IntegerFields.Any(x => x == Header))
                        {
                            var Col = Table.Columns.Add(Header, typeof(int));
                            Col.AllowDBNull = true;
                        }
                        else
                        {
                            var Col = Table.Columns.Add(Header);
                            Col.AllowDBNull = true;
                        }
                    }
                    else
                    {
                        var Col = Table.Columns.Add(Header);
                        Col.AllowDBNull = true;
                    }
                        
                }
                CurrentIndex = CurrentIndex + 1;
            }

            for (var Counter = CurrentIndex; Counter < Arrays.Count; Counter++)
            {
                //Table.Rows.Add(Arrays[Counter]);
                var NewRow = Table.NewRow();

                for (var i = 0; i < Table.Columns.Count; i++)
                {
                    if (AdditionalParams.DateTimeFields.Any(x => x == Table.Columns[i].ColumnName))
                    {
                        if (!String.IsNullOrWhiteSpace(Arrays[Counter][i]))
                        {
                            NewRow[i] = Convert.ToDateTime(Arrays[Counter][i]);
                        }
                        else
                        {
                            NewRow[i] = DBNull.Value;
                        }
                        
                    }
                    else if (AdditionalParams.DecimalFields.Any(x => x == Table.Columns[i].ColumnName))
                    {
                        if (!String.IsNullOrWhiteSpace(Arrays[Counter][i]))
                        {
                            NewRow[i] = Convert.ToDecimal(Arrays[Counter][i]);
                        }
                        else
                        {
                            NewRow[i] = DBNull.Value;
                        }
                    }
                    else if (AdditionalParams.DoubleFields.Any(x => x == Table.Columns[i].ColumnName))
                    {
                        if (!String.IsNullOrWhiteSpace(Arrays[Counter][i]))
                        {
                            NewRow[i] = Convert.ToDouble(Arrays[Counter][i]);
                        }
                        else
                        {
                            NewRow[i] = DBNull.Value;
                        }
                    }
                    else if (AdditionalParams.IntegerFields.Any(x => x == Table.Columns[i].ColumnName))
                    {
                        if (!String.IsNullOrWhiteSpace(Arrays[Counter][i]))
                        {
                            NewRow[i] = Convert.ToInt32(Arrays[Counter][i]);
                        }
                        else
                        {
                            NewRow[i] = DBNull.Value;
                        }
                    }
                    else
                    {
                        NewRow[i] = Arrays[Counter][i];
                    }
                }

                Table.Rows.Add(NewRow);
            }

            foreach( DataColumn Header in Table.Columns)
            {
                Header.ColumnName = Header.ColumnName.Trim().Replace(" ", String.Empty);
            }

            return Table;
        }        

        public string GetJSONFromDataTable( DataTable Table)
        {
            var JSON = string.Empty;



            return JSON;
        }
    }
}
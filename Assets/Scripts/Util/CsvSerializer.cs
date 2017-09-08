//===================================================================================
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
//===================================================================================
// This code is released under the terms of the CPOL license, 
//===================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using LuaInterface;

namespace Csv.Serialization
{
    /// <summary>
    /// Serialize and Deserialize Lists of any object type to CSV.
    /// </summary>
    public class CsvSerializer<T> where T : class, new()
    {
        #region Fields
        private bool _ignoreEmptyLines = true;

        private bool _ignoreReferenceTypesExceptString = true;

        private string _newlineReplacement = ( (char)0x254 ).ToString();

        private List<PropertyInfo> _properties;

        private string _replacement = ( (char)0x255 ).ToString();

        private string _rowNumberColumnTitle = "RowNumber";

        private char _separator = ',';

        private bool _useEofLiteral = false;

        private bool _useLineNumbers = true;

        private bool _useTextQualifier = false;

        #endregion Fields

        #region Properties
        public bool IgnoreEmptyLines
        {
            get { return _ignoreEmptyLines; }
            set { _ignoreEmptyLines = value; }
        }

        public bool IgnoreReferenceTypesExceptString
        {
            get { return _ignoreReferenceTypesExceptString; }
            set { _ignoreReferenceTypesExceptString = value; }
        }

        public string NewlineReplacement
        {
            get { return _newlineReplacement; }
            set { _newlineReplacement = value; }
        }

        public string Replacement
        {
            get { return _replacement; }
            set { _replacement = value; }
        }

        public string RowNumberColumnTitle
        {
            get { return _rowNumberColumnTitle; }
            set { _rowNumberColumnTitle = value; }
        }

        public char Separator
        {
            get { return _separator; }
            set { _separator = value; }
        }

        public bool UseEofLiteral
        {
            get { return _useEofLiteral; }
            set { _useEofLiteral = value; }
        }

        public bool UseLineNumbers
        {
            get { return _useLineNumbers; }
            set { _useLineNumbers = value; }
        }

        public bool UseTextQualifier
        {
            get { return _useTextQualifier; }
            set { _useTextQualifier = value; }
        }

        #endregion Properties

        /// <summary>
        /// Csv Serializer
        /// Initialize by selected properties from the type to be de/serialized
        /// </summary>
        public CsvSerializer( bool ignoreReferenceTypesExceptString = false )
        {
            _ignoreReferenceTypesExceptString = ignoreReferenceTypesExceptString;
            var type = typeof( T );

            var properties = type.GetProperties( BindingFlags.Public | BindingFlags.Instance
                | BindingFlags.GetProperty | BindingFlags.SetProperty );

            // 원본  
            //			var q = properties.AsQueryable();
            //			if( IgnoreReferenceTypesExceptString )
            //			{
            //				q = q.Where(a => a.PropertyType.IsValueType || a.PropertyType.Name == "String");
            //			}
            //			
            //			var r = from a in q
            //					where a.GetCustomAttributes( typeof( CsvIgnoreAttribute ), true ) == null
            //					orderby a.Name
            //					select a;

            // 수정본   
            _properties = new List<PropertyInfo>();
            if( ignoreReferenceTypesExceptString )
            {
                foreach( var prop in properties )
                {
                    if( prop.GetCustomAttributes( typeof( CsvIgnoreAttribute ), true ) == null &&
                        prop.PropertyType.IsValueType || prop.PropertyType.Name == "String" )
                        _properties.Add( prop );
                }
            }
            else
            {
                foreach( var prop in properties )
                {
                    var att = prop.GetCustomAttributes( typeof( CsvIgnoreAttribute ), true );

                    if( att.Length == 0 )
                        _properties.Add( prop );
                }
            }
        }

        public IList<T> Deserialize( Stream stream )
        {
            string[] columns;
            string[] rows;
            try
            {
                using( var sr = new StreamReader( stream ) )
                {
                    columns = sr.ReadLine().Split( Separator );
                    rows = sr.ReadToEnd().Split( new string[] { Environment.NewLine }, StringSplitOptions.None );
                }
                return Deserialize( columns, rows );
            }
            catch( Exception ex )
            {
                throw new InvalidCsvFormatException( "The CSV File is Invalid. See Inner Exception for more inoformation.", ex );
            }
        }

        public IList<T> Deserialize( string text )
        {
            string[] columns;
            string[] rows;

            try
            {
                using( var sr = new StringReader( text ) )
                {
                    columns = sr.ReadLine().Split( Separator );
                    rows = sr.ReadToEnd().Split( new string[] { Environment.NewLine }, StringSplitOptions.None );
                }
                return Deserialize( columns, rows );
            }
            catch( Exception ex )
            {
                throw new InvalidCsvFormatException( "The CSV File is Invalid. See Inner Exception for more inoformation.", ex );
            }
        }

        public IList<T> DeserializeStream(string text)
        {
            string[] columns;
            string[] rows;

            try
            {
                using (var sr = new StringReader(text))
                {
                    columns = sr.ReadLine().Split(Separator);

                    // 读取无用的类型标识
                    sr.ReadLine();
                    sr.ReadLine();

                    rows = sr.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                }
                return DeserializeStream(columns, rows);
            }
            catch (Exception ex)
            {
                throw new InvalidCsvFormatException("The CSV File is Invalid. See Inner Exception for more inoformation.", ex);
            }
        }

        /// <summary>
        /// Deserialize
        /// </summary>
        /// <param name="stream">stream</param>
        /// <returns></returns>
        public IList<T> Deserialize( string[] columns, string[] rows )
        {
            var data = new List<T>();

            for( int row = 0 ; row < rows.Length ; row++ )
            {
                var line = rows[row];

                if( IgnoreEmptyLines && IsNullOrWhiteSpace( line ) )
                {
                    continue;
                }
                else if( !IgnoreEmptyLines && IsNullOrWhiteSpace( line ) )
                {
                    throw new InvalidCsvFormatException( string.Format( @"Error: Empty line at line number: {0}", row ) );
                }
                var parts = line.Split( Separator );

                var firstColumnIndex = UseLineNumbers ? 2 : 1;
                if( parts.Length == firstColumnIndex && parts[firstColumnIndex - 1] != null && parts[firstColumnIndex - 1] == "EOF" )
                {
                    break;
                }

                var datum = new T();
                //				GLog.LogWarning( "csv name:{0}, length {1}", parts[1], parts.Length );
                var start = UseLineNumbers ? 1 : 0;
                for( int i = start ; i < parts.Length ; i++ )
                {
                    var value = parts[i];
                    var column = columns[i];

                    // 원본   
                    // continue of deviant RowNumber column condition
                    // this allows for the deserializer to implicitly ignore the RowNumber column
                    //					if (column.Equals(RowNumberColumnTitle) && !_properties.Any(a => a.Name.Equals(RowNumberColumnTitle)))
                    //						continue;

                    // 수정본    
                    if( column.Equals( RowNumberColumnTitle ) )
                    {
                        foreach( var prop in _properties )
                        {
                            if( prop.Name.Equals( RowNumberColumnTitle ) )
                                break;
                        }
                        continue;
                    }


                    value = value
                        .Replace( Replacement, Separator.ToString() )
                        .Replace( NewlineReplacement, Environment.NewLine ).Trim();

                    var p = _properties.Find( a => a.Name.Equals( column, StringComparison.InvariantCultureIgnoreCase ) );


                    /// ignore property csv column, Property not found on targing type
                    if( p == null || string.IsNullOrEmpty( value ) )
                    {
                        continue;
                    }

                    if( UseTextQualifier )
                    {
                        if( value.IndexOf( "\"" ) == 0 )
                            value = value.Substring( 1 );

                        if( value[value.Length - 1].ToString() == "\"" )
                            value = value.Substring( 0, value.Length - 1 );
                    }

                    
	                try
	                {
						var converter = TypeDescriptor.GetConverter( p.PropertyType );
						var convertedvalue = converter.ConvertFrom( value );
							//Logger.Log( "csv - " + value );
						p.SetValue( datum, convertedvalue, null );
	                }
	                catch( Exception )
	                {
                        Debugger.LogError( string.Format( "csv value convert error ( value = {0} )", value ) );
		                throw;
	                }
                }

                data.Add( datum );
            }

            return data;
        }


        public IList<T> DeserializeStream(string[] columns, string[] rows)
        {
            var data = new List<T>();
            //Debugger.Log("UseLineNumbers : " + UseLineNumbers);
            for (int row = 0; row < rows.Length; row++)
            {
                var line = rows[row];

                if (IgnoreEmptyLines && IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                else if (!IgnoreEmptyLines && IsNullOrWhiteSpace(line))
                {
                    throw new InvalidCsvFormatException(string.Format(@"Error: Empty line at line number: {0}", row));
                }
                var parts = line.Split(Separator);

                var firstColumnIndex = UseLineNumbers ? 2 : 1;
                if (parts.Length == firstColumnIndex && parts[firstColumnIndex - 1] != null && parts[firstColumnIndex - 1] == "EOF")
                {
                    break;
                }

                var datum = new T();
                //				GLog.LogWarning( "csv name:{0}, length {1}", parts[1], parts.Length );
                var start = UseLineNumbers ? 1 : 0;
                int len = parts.Length;
                for (int i = start; i < len; i++)
                {
                    var value = parts[i];
                    var column = columns[i];

                    // 원본   
                    // continue of deviant RowNumber column condition
                    // this allows for the deserializer to implicitly ignore the RowNumber column
                    //					if (column.Equals(RowNumberColumnTitle) && !_properties.Any(a => a.Name.Equals(RowNumberColumnTitle)))
                    //						continue;

                    // 수정본    
                    if (column.Equals(RowNumberColumnTitle))
                    {
                        foreach (var prop in _properties)
                        {
                            if (prop.Name.Equals(RowNumberColumnTitle))
                                break;
                        }
                        continue;
                    }

                    value = value
                        .Replace(Replacement, Separator.ToString())
                        .Replace(NewlineReplacement, Environment.NewLine).Trim();
                    //if (datum is MSEnemyInfo)
                    //{
                    //    Logger.Log(start + ") " + column + " / " + len + "//" + _properties.Count +" , " + columns.Length + " [" + i + "]value : " + value);
                    //}
                    //
                    //var p = _properties.Find(a => a.Name.Equals(column, StringComparison.InvariantCultureIgnoreCase));
                    var p = _properties[i];

                    /// ignore property csv column, Property not found on targing type
                    if (p == null || string.IsNullOrEmpty(value))
                    {
                        continue;
                    }

                    if (UseTextQualifier)
                    {
                        if (value.IndexOf("\"") == 0)
                            value = value.Substring(1);

                        if (value[value.Length - 1].ToString() == "\"")
                            value = value.Substring(0, value.Length - 1);
                    }

                    //非简繁体中文将##替换为 半角逗号“,”
#if !ZH_CN && !ZH_TW  
                    value = value.Replace("##", ",");
#endif
                    var converter = TypeDescriptor.GetConverter(p.PropertyType);
                    var convertedvalue = converter.ConvertFrom(value);

                    //Logger.Log( "csv - " + value );
                    p.SetValue(datum, convertedvalue, null);
                }

                data.Add(datum);
            }

            return data;
        }
        /// <summary>
        /// Serialize
        /// </summary>
        /// <param name="stream">stream</param>
        /// <param name="data">data</param>

        //원본.
        /*
		public void Serialize(Stream stream, IList<T> data)
		{
			var sb = new StringBuilder();
			var values = new List<string>();

			sb.AppendLine(GetHeader());

			var row = 1;
			foreach (var item in data)
			{
				values.Clear();

				if (UseLineNumbers)
				{
					values.Add(row++.ToString());
				}

				foreach (var p in _properties)
				{
					var raw = p.GetValue(item, null);
					var value = raw == null ? "" :
						raw.ToString()
						.Replace(Separator.ToString(), Replacement)
						.Replace(Environment.NewLine, NewlineReplacement);

					if (UseTextQualifier)
					{
						value = string.Format("\"{0}\"", value);
					}

					values.Add(value);
				}
				sb.AppendLine(string.Join(Separator.ToString(), values.ToArray()));
			}

			if (UseEofLiteral)
			{
				values.Clear();

				if (UseLineNumbers)
				{
					values.Add(row++.ToString());
				}

				values.Add("EOF");

				sb.AppendLine(string.Join(Separator.ToString(), values.ToArray()));
			}

			using (var sw = new StreamWriter(stream))
			{
				sw.Write(sb.ToString().Trim());
			}
		}
        */

        // 수정본 by 김응용.
        public void Serialize( Stream stream, IList<T> data )
        {
            string csvText = Serialize( data );

            using( var sw = new StreamWriter( stream ) )
            {
                sw.Write( csvText );
            }
        }

        public string Serialize( IList<T> data )
        {
            var sb = new StringBuilder();
            var values = new List<string>();

            sb.AppendLine( GetHeader() );

            var row = 1;
            foreach( var item in data )
            {
                values.Clear();

                if( UseLineNumbers )
                {
                    values.Add( row++.ToString() );
                }

                foreach( var p in _properties )
                {
                    var raw = p.GetValue( item, null );
                    var value = raw == null ? "" :
                        raw.ToString()
                        .Replace( Separator.ToString(), Replacement )
                        .Replace( Environment.NewLine, NewlineReplacement );

                    if( UseTextQualifier )
                    {
                        value = string.Format( "\"{0}\"", value );
                    }

                    values.Add( value );
                }
                sb.AppendLine( string.Join( Separator.ToString(), values.ToArray() ) );
            }

            if( UseEofLiteral )
            {
                values.Clear();

                if( UseLineNumbers )
                {
                    values.Add( row++.ToString() );
                }

                values.Add( "EOF" );

                sb.AppendLine( string.Join( Separator.ToString(), values.ToArray() ) );
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Get Header
        /// </summary>
        /// <returns></returns>
        private string GetHeader()
        {
            // 원본  
            string[] header = null;
            int startCount = 0;
            if( UseLineNumbers )
            {
                header = new string[_properties.Count + 1];
                header[0] = RowNumberColumnTitle;
                startCount = 1;
            }
            else
            {
                header = new string[_properties.Count];
            }

            for( int i = startCount ; i < _properties.Count ; ++i )
            {
                header[i] = _properties[i].Name;
            }

            return string.Join( Separator.ToString(), header );
        }

        static bool IsNullOrWhiteSpace( string value )
        {
            if( ( value == null ) || ( value.Length == 0 ) )
                return true;
            foreach( char c in value )
                if( !Char.IsWhiteSpace( c ) )
                    return false;
            return true;
        }


        public IList<T> DeserializeHash(string text)
        {
            string[] columns;
            string[] keys;
            string[] rows;

            try
            {
                //Logger.Log("DeserializeHash : " + text);
                using( var sr = new StringReader( text ) )
                {
                    columns = sr.ReadLine().Split( Separator );
                    keys = sr.ReadLine().Split(Separator);
                    rows = sr.ReadToEnd().Split( new string[] { Environment.NewLine }, StringSplitOptions.None );
                }
                return Deserialize( columns, keys, rows );
            }
            catch( Exception ex )
            {
                throw new InvalidCsvFormatException( "The CSV File is Invalid. See Inner Exception for more information.", ex );
            }
        }

        public IList<T> Deserialize(string[] columns, string[] keys, string[] rows)
        {
            const int HEX_LEN = 4;
            int columns_len = columns.Length;
            int col_data_len = (columns_len * HEX_LEN);
            int rows_len = rows.Length;

            var data = new List<T>();

            //StringBuilder strb = new StringBuilder();
            //foreach (string col in columns)
            //{
            //    strb.Append(col);
            //    strb.Append(",");
            //}
            //strb.Append("\n");
            //foreach (string key in keys)
            //{
            //    strb.Append(key);
            //    strb.Append(",");
            //}
            //Logger.Log(strb.ToString());
            // 데이터 분리
            for (int row = 0; row < rows_len; row++) // 1 줄 임
            {
                var row_data = rows[row];

                int row_data_len = row_data.Length;
                int line_len = (row_data_len / (columns_len * HEX_LEN));

                // 라인 분리
                for (int line = 0; line < line_len; line++)
                {
                    var datum = new T();
                    for (int col = 0; col < columns_len; col++)
                    {
                        
                        var hex_data = row_data.Substring((col * HEX_LEN) + (line * col_data_len), HEX_LEN);
                        int index = Convert.ToInt32(hex_data, 16);
                        var value = keys[index];

                        //Logger.Log("hex_data ("+ index + ")" + columns_len + "/ " + line_len + "||" + col + "," + HEX_LEN + "," + line + "," + col_data_len + " // " + hex_data);

                        var p = _properties[col];

                        if (p == null || string.IsNullOrEmpty(value))
                        {
                            continue;
                        }

                        var converter = TypeDescriptor.GetConverter(p.PropertyType);
                        var convertedvalue = converter.ConvertFrom(value);

                        //Logger.Log( "csv - " + value );
                        p.SetValue(datum, convertedvalue, null);

                    }
                    data.Add(datum);
                }
            }

            return data;
        }
    }

    public class CsvIgnoreAttribute : Attribute { }

    public class InvalidCsvFormatException : Exception
    {
        /// <summary>
        /// Invalid Csv Format Exception
        /// </summary>
        /// <param name="message">message</param>
        public InvalidCsvFormatException( string message )
            : base( message )
        {
        }

        public InvalidCsvFormatException( string message, Exception ex )
            : base( message, ex )
        {
        }
    }


}

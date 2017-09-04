using System.Collections.Generic;
using System.Text;

namespace SiLanguage
{

    public enum SiTokenTypes
    {
        siComment, siDatatype, siIdent, siKeyword, siNumber, siOther, siPunctuation, siString, siPlaceholder, siSqlKeyword, siSqlString, siSqlFuntion, siSqlOperator
    }
    public struct SiToken
    {
        public string value;
        public SiTokenTypes tokenType;
        public int position;
    }
    internal class SiTokenizer
    {
        const int START = 0, IDENT = 1, WHITESPACE = 2, NUMBER = 3, PUNCTUATION = 4, AMP = 5,
        MULTICOMMENT = 6, LINECOMMENT = 7, SLASH = 8,
        ISMULTIEND = 9, DOLLAR = 10, DSTRING = 11, SSTRING = 12, IDENTQUOTE = 13, PLACEHOLDER = 14;
        internal static string Keywords;
        internal static string DataTypes;
        internal static string SqlKeywords;
        internal static string SqlFunctions;
        internal static string SqlOperators;
        const string whitespace = " \t";
        const string identstart = "abcdefghijklmnopqrstuvwxyz_[]";
        const string numberstart = "0123456789";
        const string numberextra = ".";
        const string punctuation = "`~!%^*()-+={};|\\<>?,.";
        internal StringBuilder builder = new StringBuilder();
        internal int _state = START;
        internal int _stateType = START;
        private static string _lcKeywords;
        private static string _lcDataTypes;
        private static string _lcSqlKeywords;
        private static string _lcSqlFuntions;
        private static string _lcSqlOperators;

        static SiTokenizer()
        {
            Keywords = ":" + string.Join(":", SiKeywordMap.Keys) + ":";
            DataTypes = ":" + string.Join(":", SiDatatypeMap.Keys) + ":";
            SqlKeywords = ":" + string.Join(":", SqlKeywordMap.Keys) + ":";
            SqlFunctions = ":" + string.Join(":", SqlFunctionMap.Keys) + ":";
            SqlOperators = ":" + string.Join(":", SqlOperatorMap.Keys) + ":";

            _lcKeywords = Keywords.ToLower();
            _lcDataTypes = DataTypes.ToLower();
            _lcSqlKeywords = SqlKeywords.ToLower();
            _lcSqlFuntions = SqlFunctions.ToLower();
            _lcSqlOperators = SqlOperators.ToLower();
        }

        internal SiTokenTypes tokenType(string value, bool isSql)
        {
            string lookfor = "";
            switch (_stateType)
            {
                case WHITESPACE:
                case START:
                    return SiTokenTypes.siOther;
                case IDENT:
                case IDENTQUOTE:
                    lookfor = ":" + value + ":";
                    if (isSql)
                    {
                        if (_lcSqlKeywords.IndexOf(lookfor) >= 0)
                            return SiTokenTypes.siSqlKeyword;
                        if (_lcSqlFuntions.IndexOf(lookfor) >= 0)
                            return SiTokenTypes.siSqlFuntion;
                        if (_lcSqlOperators.IndexOf(lookfor) >= 0)
                            return SiTokenTypes.siSqlOperator;
                        if (_lcKeywords.IndexOf(lookfor) >= 0)
                            return SiTokenTypes.siKeyword;
                        if (_lcDataTypes.IndexOf(lookfor) >= 0)
                            return SiTokenTypes.siDatatype;
                    }
                    else
                    {
                        if (_lcKeywords.IndexOf(lookfor) >= 0)
                            return SiTokenTypes.siKeyword;
                        if (_lcDataTypes.IndexOf(lookfor) >= 0)
                            return SiTokenTypes.siDatatype;
                        if (_lcSqlKeywords.IndexOf(lookfor) >= 0)
                            return SiTokenTypes.siSqlKeyword;
                        if (_lcSqlFuntions.IndexOf(lookfor) >= 0)
                            return SiTokenTypes.siSqlFuntion;
                        if (_lcSqlOperators.IndexOf(lookfor) >= 0)
                            return SiTokenTypes.siSqlOperator;

                    }
                    return SiTokenTypes.siIdent;
                case NUMBER:
                    return SiTokenTypes.siNumber;
                case PUNCTUATION:
                case SLASH:
                case DOLLAR:
                case AMP:
                    return SiTokenTypes.siPunctuation;
                case MULTICOMMENT:
                case LINECOMMENT:
                case ISMULTIEND:
                    return SiTokenTypes.siComment;
                case DSTRING:
                    return SiTokenTypes.siString;
                case SSTRING:
                    return SiTokenTypes.siSqlString;
                case PLACEHOLDER:
                    return SiTokenTypes.siPlaceholder;
            }
            return SiTokenTypes.siOther;
        }
        internal void storeToken(List<SiToken> tokens, StringBuilder builder, char ch, int position, bool isSql)
        {
            var token = new SiToken();
            token.value = builder.ToString();
            if (token.value.Trim().Length > 0)
            {
                token.tokenType = tokenType(token.value, isSql);
                token.position = position;
                tokens.Add(token);
            }
            builder.Clear();
            if (ch != '\0')
                builder.Append(ch);
        }
        internal int prevPosition = 0;
        internal void getTokens(List<SiToken> tokens, string line, int position, bool isSql)
        {
            if (prevPosition > position)
                _state = START;
            prevPosition = position;
            char[] chs = line.ToCharArray();
            for (int i = 0; i < chs.Length; i++)
            {
                char ch = chs[i];
                switch (_state)
                {
                    case START:
                        if (whitespace.IndexOf(ch) >= 0)
                        {
                            storeToken(tokens, builder, ch, i, isSql);
                            _stateType = _state = WHITESPACE;
                            continue;
                        }
                        if (ch == 'l')
                        {
                            storeToken(tokens, builder, ch, i, isSql);
                            _stateType = _state = IDENTQUOTE;
                            continue;
                        }
                        if (identstart.IndexOf(ch) >= 0)
                        {
                            storeToken(tokens, builder, ch, i, isSql);
                            _stateType = _state = IDENT;
                            continue;
                        }
                        if (numberstart.IndexOf(ch) >= 0)
                        {
                            storeToken(tokens, builder, ch, i, isSql);
                            _stateType = _state = NUMBER;
                            continue;
                        }
                        if (ch == '&' || ch == ':')
                        {
                            storeToken(tokens, builder, ch, i, isSql);
                            _stateType = _state = PLACEHOLDER;
                            continue;
                        }
                        if (ch == '#')
                        {
                            storeToken(tokens, builder, ch, i, isSql);
                            _stateType = _state = LINECOMMENT;
                            continue;
                        }
                        if (ch == '/')
                        {
                            storeToken(tokens, builder, ch, i, isSql);
                            _stateType = _state = SLASH;
                            continue;
                        }
                        if (ch == '$' || ch == '@')
                        {
                            storeToken(tokens, builder, ch, i, isSql);
                            _stateType = _state = DOLLAR;
                            continue;
                        }
                        if (ch == '"')
                        {
                            storeToken(tokens, builder, ch, i, isSql);
                            _stateType = _state = DSTRING;
                            continue;
                        }
                        if (ch == '\'')
                        {
                            storeToken(tokens, builder, ch, i, isSql);
                            _stateType = _state = SSTRING;
                            continue;
                        }
                        if (punctuation.IndexOf(ch) >= 0)
                        {
                            storeToken(tokens, builder, ch, i, isSql);
                            _stateType = _state = PUNCTUATION;
                            continue;
                        }
                        break;
                    case WHITESPACE:
                        if (whitespace.IndexOf(ch) < 0)
                            goto case START;
                        break;
                    case IDENT:
                        if (identstart.IndexOf(ch) < 0 && numberstart.IndexOf(ch) < 0)
                            goto case START;
                        break;
                    case IDENTQUOTE:
                        if (identstart.IndexOf(ch) < 0 && numberstart.IndexOf(ch) < 0 && ch != '\'')
                            goto case START;
                        break;
                    case NUMBER:
                        if (numberstart.IndexOf(ch) < 0 && numberextra.IndexOf(ch) < 0)
                            goto case START;
                        break;
                    case AMP:
                        if (identstart.IndexOf(ch) < 0 && numberstart.IndexOf(ch) < 0 && ch != ';')
                            goto case START;
                        break;
                    case LINECOMMENT:
                        break;
                    case MULTICOMMENT:
                        if (ch == '*')
                            _state = ISMULTIEND;
                        break;
                    case ISMULTIEND:
                        if (ch == '/')
                            _state = START;
                        else if (ch != '*')
                            _state = MULTICOMMENT;
                        break;
                    case DSTRING:
                        if (ch == '"')
                            _state = START;
                        break;
                    case SSTRING:
                        if (ch == '\'')
                            _state = START;
                        break;
                    case SLASH:
                        if (ch == '/')
                            _stateType = _state = LINECOMMENT;
                        else if (ch == '*')
                            _stateType = _state = MULTICOMMENT;
                        else
                        {
                            _stateType = PUNCTUATION;
                            goto case START;
                        }
                        break;
                    case DOLLAR:
                        if (identstart.IndexOf(ch) < 0 && numberstart.IndexOf(ch) < 0)
                            goto case START;
                        break;
                    case PUNCTUATION:
                        if (punctuation.IndexOf(ch) < 0)
                            goto case START;
                        break;
                    case PLACEHOLDER:
                        if (identstart.IndexOf(ch) < 0 && numberstart.IndexOf(ch) < 0)
                            goto case START;
                        break;
                }
                builder.Append(ch);
            }
            if (_state == ISMULTIEND)
                _state = MULTICOMMENT;
            storeToken(tokens, builder, '\0', line.Length, isSql);
            if (_state != MULTICOMMENT)
                _state = START;
        }

        internal static Dictionary<string, string> SiKeywordMap = new Dictionary<string, string>
        {
            ["All"] = "All",
            ["BulkInsert"] = "BulkInsert",
            ["BulkUpdate"] = "BulkUpdate",
            ["CASCADE"] = "Cascade",
            ["CHECK"] = "Check",
            ["COUNT"] = "Count",
            ["CONST"] = "Const",
            ["CURSOR"] = "Cursor",
            ["DATABASE"] = "Database",
            ["DECLARE"] = "Declare",
            ["Delete"] = "Delete",
            ["DeleteAll"] = "DeleteAll",
            ["DeleteOne"] = "DeleteOne",
            ["DYNAMIC"] = "Dynamic",
            ["EXECUTE"] = "Execute",
            ["Exists"] = "Exists",
            ["Flags"] = "Flags",
            ["GRANT"] = "Grant",
            ["IMPORT"] = "Import",
            ["IN"] = "In",
            ["INOUT"] = "InOut",
            ["INPUT"] = "Input",
            ["Insert"] = "Insert",
            ["KEY"] = "Key",
            ["LINK"] = "Link",
            ["MERGE"] = "Merge",
            ["MULTIPLE"] = "Multiple",
            ["NAMES"] = "Names",
            ["NOT"] = "Not",
            ["NULL"] = "Null",
            ["Options"] = "Options",
            ["ORDER"] = "Order",
            ["OUTPUT"] = "Output",
            ["PACKAGE"] = "Package",
            ["PASSWORD"] = "Password",
            ["PRIMARY"] = "Primary",
            ["PROC"] = "Proc",
            ["READONLY"] = "ReadOnly",
            ["RETURNING"] = "Returning",
            ["SCHEMA"] = "Schema",
            ["Select"] = "Select",
            ["SelectBy"] = "SelectBy",
            ["SelectAll"] = "SelectAll",
            ["SelectOne"] = "SelectOne",
            ["SERVER"] = "Server",
            ["Single"] = "Single",
            ["SQL"] = "SQL",
            ["SPROC"] = "SProc",
            ["Standard"] = "Standard",
            ["TABLE"] = "Table",
            ["TO"] = "To",
            ["UNIQUE"] = "Unique",
            ["Update"] = "Update",
            ["UpdateBy"] = "UpdateBy",
            ["UTF8"] = "UTF8",
            ["VIEW"] = "View",
            ["ENDCODE"] = "EndCode",
            ["ENDDATA"] = "EndData",
            ["SQLCODE"] = "SQLCode",
            ["SQLDATA"] = "SQLData",
            ["DEFAULT"] = "Default",
            ["MaxTmStamp"] = "MaxTmStamp",
            ["FOR"] = "For",
            ["FROM"] = "From",
            ["WHERE"] = "Where",
            ["SelectOneBy"] = "SelectOneBy",
            ["UpdateFor"] = "UpdateFor",
            ["DeleteBy"] = "DeleteBy",
        };

        internal static Dictionary<string, string> SiDatatypeMap = new Dictionary<string, string>
        {
            ["ANSICHAR"] = "AnsiChar",
            ["AUTOTIMESTAMP"] = "AutoTimeStamp",
            ["BIGIDENTITY"] = "BigIdentity",
            ["BIGSEQUENCE"] = "BigSequence",
            ["BIGXML"] = "BigXML",
            ["BLOB"] = "Blob",
            ["BOOLEAN"] = "Boolean",
            ["BYTE"] = "Byte",
            ["CHAR"] = "Char",
            ["DATE"] = "Date",
            ["DATETIME"] = "DateTime",
            ["DOUBLE"] = "Double",
            ["FLOAT"] = "Float",
            ["IDENTITY"] = "Identity",
            ["INT"] = "Int",
            ["LONG"] = "Long",
            ["MONEY"] = "Money",
            ["SEQUENCE"] = "Sequence",
            ["SHORT"] = "Short",
            ["TIME"] = "Time",
            ["TIMESTAMP"] = "TimeStamp",
            ["TLOB"] = "Tlob",
            ["UID"] = "UID",
            ["USERID"] = "UserId",
            ["USERSTAMP"] = "UserStamp",
            ["WANSICHAR"] = "WAnsiChar",
            ["WCHAR"] = "WChar",
            ["XML"] = "XML",
            ["CLOB"] = "Clob",
            ["SMALLINT"] = "SmallInt",
            ["TINYINT"] = "TinyInt",
            ["ANSI"] = "Ansi",
            ["BIGINTEGER"] = "BigInteger",
            ["INTEGER"] = "Integer"
        };

        internal static Dictionary<string, string> SqlKeywordMap = new Dictionary<string, string>
        {
            ["ABSOLUTE"] = "Absolute",
            ["ACTION"] = "Action",
            ["ADD"] = "Add",
            ["ALTER"] = "Alter",
            ["AS"] = "As",
            ["ASC"] = "Asc",
            ["AUTHORIZATION"] = "Authorization",
            ["BACKUP"] = "Backup",
            ["BEGIN"] = "Begin",
            ["BIT"] = "Bit",
            ["BREAK"] = "Break",
            ["BROWSE"] = "Browse",
            ["BULK"] = "Bulk",
            ["BY"] = "By",
            ["CASCADE"] = "Cascade",
            ["CASE"] = "Case",
            ["CATALOG"] = "Catalog",
            ["CHAR"] = "Char",
            ["CHARACTER"] = "Character",
            ["CHECK"] = "Check",
            ["CHECKPOINT"] = "Checkpoint",
            ["CLOSE"] = "Close",
            ["CLUSTERED"] = "Clustered",
            ["COLUMN"] = "Column",
            ["COMMIT"] = "Commit",
            ["COMPUTE"] = "Compute",
            ["CONNECT"] = "Connect",
            ["CONSTRAINT"] = "Constraint",
            ["CONTAINSTABLE"] = "ContainsTable",
            ["CONTINUE"] = "Continue",
            ["CREATE"] = "Create",
            ["CURRENT"] = "Current",
            ["CURRENT_DATE"] = "Current_Date",
            ["CURSOR"] = "Cursor",
            ["DATABASE"] = "Database",
            ["DATE"] = "Date",
            ["DBCC"] = "DBCC",
            ["DEALLOCATE"] = "Deallocate",
            ["DEC"] = "DEC",
            ["DECIMAL"] = "Decimal",
            ["DECLARE"] = "Declare",
            ["DEFAULT"] = "Default",
            ["DELETE"] = "Delete",
            ["DENY"] = "Deny",
            ["DESC"] = "Desc",
            ["DISK"] = "Disk",
            ["DISTINCT"] = "Distinct",
            ["DISTRIBUTED"] = "Distributed",
            ["DOUBLE"] = "Double",
            ["DROP"] = "Drop",
            ["DUMP"] = "Dump",
            ["ELSE"] = "Else",
            ["END"] = "End",
            ["ERRLVL"] = "ErrLvl",
            ["ESCAPE"] = "Escape",
            ["EXCEPT"] = "Except",
            ["EXEC"] = "Exec",
            ["EXECUTE"] = "Execute",
            ["EXIT"] = "Exit",
            ["EXTERNAL"] = "External",
            ["FETCH"] = "Fetch",
            ["FILE"] = "File",
            ["FILLFACTOR"] = "FillFactor",
            ["FIRST"] = "First",
            ["FLOAT"] = "Float",
            ["FOR"] = "For",
            ["FOREIGN"] = "Foreign",
            ["FREETEXT"] = "FreeText",
            ["FREETEXTTABLE"] = "FreeTextTable",
            ["FROM"] = "From",
            ["FULL"] = "Full",
            ["FUNCTION"] = "Function",
            ["GET"] = "Get",
            ["GLOBAL"] = "Global",
            ["GO"] = "Go",
            ["GOTO"] = "GoTo",
            ["GRANT"] = "Grant",
            ["GROUP"] = "Group",
            ["HAVING"] = "Having",
            ["HOLDLOCK"] = "HoldLock",
            ["IDENTITY"] = "Identity",
            ["IDENTITYCOL"] = "IdentityCol",
            ["IDENTITY_INSERT"] = "Identity_Insert",
            ["IF"] = "If",
            ["IMMEDIATE"] = "Immediate",
            ["INCLUDE"] = "Include",
            ["INDEX"] = "Index",
            ["INSENSITIVE"] = "Insensitive",
            ["INSERT"] = "Insert",
            ["INT"] = "Int",
            ["INTEGER"] = "Integer",
            ["INTERSECT"] = "Intersect",
            ["INTO"] = "Into",
            ["ISOLATION"] = "Isolation",
            ["KEY"] = "Key",
            ["KILL"] = "Kill",
            ["LANGUAGE"] = "Language",
            ["LAST"] = "Last",
            ["LEVEL"] = "Level",
            ["LINENO"] = "LineNo",
            ["LOAD"] = "Load",
            ["LOCAL"] = "Local",
            ["MERGE"] = "Merge",
            ["NATIONAL"] = "National",
            ["NCHAR"] = "NChar",
            ["NEXT"] = "Next",
            ["NO"] = "No",
            ["NOCHECK"] = "NoCheck",
            ["NONCLUSTERED"] = "NonClustered",
            ["NONE"] = "None",
            ["NUMERIC"] = "Numeric",
            ["OF"] = "Of",
            ["OFF"] = "Off",
            ["OFFSETS"] = "Offsets",
            ["ON"] = "On",
            ["OPEN"] = "Open",
            ["OPENDATASOURCE"] = "OpenDataSource",
            ["OPENQUERY"] = "OpenQuery",
            ["OPENROWSET"] = "OpenRowSet",
            ["OPENXML"] = "OpenXML",
            ["OPTION"] = "Option",
            ["ORDER"] = "Order",
            ["OUTPUT"] = "Output",
            ["OVER"] = "Over",
            ["PARTIAL"] = "Partial",
            ["PERCENT"] = "Percent",
            ["PLAN"] = "Plan",
            ["PRECISION"] = "Precision",
            ["PRIMARY"] = "Primary",
            ["PRINT"] = "Print",
            ["PRIOR"] = "Prior",
            ["PROC"] = "Proc",
            ["PROCEDURE"] = "Procedure",
            ["PUBLIC"] = "Public",
            ["RAISERROR"] = "RaiseError",
            ["READ"] = "Read",
            ["READTEXT"] = "ReadText",
            ["REAL"] = "Real",
            ["RECONFIGURE"] = "Reconfigure",
            ["REFERENCES"] = "References",
            ["RELATIVE"] = "Relative",
            ["REPLICATION"] = "Replication",
            ["RESTORE"] = "Restore",
            ["RESTRICT"] = "Restrict",
            ["RETURN"] = "Return",
            ["REVERT"] = "Revert",
            ["REVOKE"] = "Revoke",
            ["ROLLBACK"] = "Rollback",
            ["ROWCOUNT"] = "RowCount",
            ["ROWGUIDCOL"] = "RowGuidCol",
            ["ROWS"] = "Rows",
            ["RULE"] = "Rule",
            ["SAVE"] = "Save",
            ["SCHEMA"] = "Schema",
            ["SCROLL"] = "Scroll",
            ["SECURITYAUDIT"] = "SecurityAudit",
            ["SELECT"] = "Select",
            ["SEMANTICKEYPHRASETABLE"] = "SemanticKeyPhraseTable",
            ["SEMANTICSIMILARITYDETAILSTABLE"] = "SemanticSimilarityDetailsTable",
            ["SEMANTICSIMILARITYTABLE"] = "SemanticSimilarityTable",
            ["SESSION"] = "Session",
            ["SET"] = "Set",
            ["SETUSER"] = "SetUser",
            ["SHUTDOWN"] = "ShutDown",
            ["SMALLINT"] = "SmallInt",
            ["SQL"] = "SQL",
            ["STATISTICS"] = "Statistics",
            ["TABLE"] = "Table",
            ["TABLESAMPLE"] = "TableSample",
            ["TEXTSIZE"] = "TextSize",
            ["THEN"] = "Then",
            ["TIME"] = "Time",
            ["TIMESTAMP"] = "Timestamp",
            ["TO"] = "To",
            ["TOP"] = "Top",
            ["TRAN"] = "Tran",
            ["TRANSACTION"] = "Transaction",
            ["TRIGGER"] = "Trigger",
            ["TRUNCATE"] = "Truncate",
            ["UNION"] = "Union",
            ["UNIQUE"] = "Unique",
            ["UPDATETEXT"] = "UpdateText",
            ["USE"] = "Use",
            ["USER"] = "User",
            ["USING"] = "Using",
            ["VALUES"] = "Values",
            ["VARCHAR"] = "VarChar",
            ["VARYING"] = "Varying",
            ["VIEW"] = "View",
            ["WAITFOR"] = "WaitFor",
            ["WHEN"] = "When",
            ["WHERE"] = "Where",
            ["WHILE"] = "While",
            ["WITH"] = "With",
            ["WITHIN"] = "Within",
            ["WRITETEXT"] = "WriteText",
            ["NOLOCK"] = "NoLock",
            ["PERSISTED"] = "Persisted",
            ["UNIQUEIDENTIFIER"] = "UniqueIdentifier",
            ["XML"] = "XML",
            ["MONEY"] = "Money",
            ["TEXT"] = "Text",
            ["DATETIME"] = "DateTime",
            ["BIGINT"] = "BigInt",
            ["SMALLMONEY"] = "SmallMoney",
            ["TINYINT"] = "TinyInt",
            ["NVARCHAR"] = "NVarChar",
            ["NTEXT"] = "NText",
            ["BINARY"] = "Binary",
            ["VARBINARY"] = "VarBinary",
            ["IMAGE"] = "Image"
        };

        internal static Dictionary<string, string> SqlFunctionMap = new Dictionary<string, string>
        {
            ["AVG"] = "Avg",
            ["MIN"] = "Min",
            ["CHECKSUM_AGG"] = "Checksum_Agg",
            ["SUM"] = "Sum",
            ["COUNT"] = "Count",
            ["STDEV"] = "STDev",
            ["COUNT_BIG"] = "Count_Big",
            ["STDEVP"] = "STDevP",
            ["GROUPING"] = "Grouping",
            ["VAR"] = "Var",
            ["GROUPING_ID"] = "Grouping_Id",
            ["VARP"] = "Varp",
            ["MAX"] = "Max",
            ["RANK"] = "Rank",
            ["NTILE"] = "NTile",
            ["DENSE_RANK"] = "Dense_Rank",
            ["ROW_NUMBER"] = "Row_Number",
            ["CAST"] = "Cast",
            ["CONVERT"] = "Convert",
            ["TRY_CONVERT"] = "Try_Convert",
            ["TRY_PARSE"] = "Try_Parse",
            ["CURSOR_STATUS"] = "Cursor_Status",
            ["SYSDATETIME"] = "SysDateTime",
            ["SYSDATETIMEOFFSET"] = "SysDateTimeOffset",
            ["SYSUTCDATETIME"] = "SysUTCDateTime",
            ["CURRENT_TIMESTAMP"] = "Current_Timestamp",
            ["GETDATE"] = "GetDate",
            ["GETUTCDATE"] = "GetUTCDate",
            ["DATENAME"] = "DateName",
            ["DATEPART"] = "DatePart",
            ["DAY"] = "Day",
            ["MONTH"] = "Month",
            ["YEAR"] = "Year",
            ["DATEFROMPARTS"] = "DateFromParts",
            ["DATETIMEFROMPARTS"] = "DateTimeFromParts",
            ["DATETIMEOFFSETFROMPARTS"] = "DateTimeOffsetFromParts",
            ["SMALLDATETIMEFROMPARTS"] = "SmallDateTimeFromParts",
            ["TIMEFROMPARTS"] = "TimeFromParts",
            ["DATEDIFF"] = "DateDiff",
            ["DATEADD"] = "DateAdd",
            ["EOMONTH"] = "EOMonth",
            ["SWITCHOFFSET"] = "SwitchOffset",
            ["TODATETIMEOFFSET"] = "ToDateTimeOffset",
            ["ISDATE"] = "IsDate",
            ["CHOOSE"] = "Choose",
            ["IIF"] = "Iif",
            ["ABS"] = "Abs",
            ["DEGREES"] = "Degrees",
            ["RAND"] = "Rand",
            ["ACOS"] = "Acos",
            ["EXP"] = "Exp",
            ["ROUND"] = "Round",
            ["ASIN"] = "AsIn",
            ["FLOOR"] = "Floor",
            ["SIGN"] = "Sign",
            ["ATAN"] = "Atan",
            ["LOG"] = "Log",
            ["SIN"] = "Sin",
            ["ATN2"] = "ATN2",
            ["LOG10"] = "Log10",
            ["SQRT"] = "Sqrt",
            ["CEILING"] = "Ceiling",
            ["PI"] = "PI",
            ["SQUARE"] = "Square",
            ["COS"] = "Cos",
            ["POWER"] = "Power",
            ["TAN"] = "Tan",
            ["COT"] = "Cot",
            ["RADIANS"] = "Radians",
            ["INDEX_COL"] = "Index_Col",
            ["APP_NAME"] = "App_Name",
            ["INDEXKEY_PROPERTY"] = "IndexKey_Property",
            ["APPLOCK_MODE"] = "AppLock_Mode",
            ["INDEXPROPERTY"] = "IndexProperty",
            ["APPLOCK_TEST"] = "AppLock_Test",
            ["ASSEMBLYPROPERTY"] = "AssemblyProperty",
            ["OBJECT_DEFINITION"] = "Object_Definition",
            ["COL_LENGTH"] = "Col_Length",
            ["OBJECT_ID"] = "Object_Id",
            ["COL_NAME"] = "Col_Name",
            ["OBJECT_NAME"] = "Object_Name",
            ["COLUMNPROPERTY"] = "ColumnProperty",
            ["OBJECT_SCHEMA_NAME"] = "Object_Schema_Name",
            ["DATABASE_PRINCIPAL_ID"] = "Database_Principal_Id",
            ["OBJECTPROPERTY"] = "ObjectProperty",
            ["DATABASEPROPERTYEX"] = "DatabasePropertyEx",
            ["OBJECTPROPERTYEX"] = "ObjectPropertyEx",
            ["DB_ID"] = "DB_Id",
            ["ORIGINAL_DB_NAME"] = "Original_DB_Name",
            ["DB_NAME"] = "DB_Name",
            ["PARSENAME"] = "ParseName",
            ["FILE_ID"] = "File_Id",
            ["SCHEMA_ID"] = "Schema_Id",
            ["FILE_IDEX"] = "File_Idex",
            ["SCHEMA_NAME"] = "Schema_Name",
            ["FILE_NAME"] = "File_Name",
            ["SCOPE_IDENTITY"] = "Scope_Identity",
            ["FILEGROUP_ID"] = "FileGroup_Id",
            ["SERVERPROPERTY"] = "ServerProperty",
            ["FILEGROUP_NAME"] = "FileGroup_Name",
            ["STATS_DATE"] = "Stats_Date",
            ["FILEGROUPPROPERTY"] = "FileGroupProperty",
            ["TYPE_ID"] = "Type_Id",
            ["FILEPROPERTY"] = "FileProperty",
            ["TYPE_NAME"] = "Type_Name",
            ["FULLTEXTCATALOGPROPERTY"] = "FullTextCatalogProperty",
            ["TYPEPROPERTY"] = "TypeProperty",
            ["FULLTEXTSERVICEPROPERTY"] = "FullTextServiceProperty",
            ["ASCII"] = "Ascii",
            ["LTRIM"] = "LTrim",
            ["SOUNDEX"] = "SoundEx",
            ["SPACE"] = "Space",
            ["CHARINDEX"] = "CharIndex",
            ["PATINDEX"] = "PatIndex",
            ["STR"] = "Str",
            ["CONCAT"] = "Concat",
            ["QUOTENAME"] = "QuoteName",
            ["STUFF"] = "Stuff",
            ["DIFFERENCE"] = "Differance",
            ["REPLACE"] = "Replace",
            ["SUBSTRING"] = "SubSring",
            ["FORMAT"] = "Format",
            ["REPLICATE"] = "Replicate",
            ["UNICODE"] = "Unicode",
            ["REVERSE"] = "Reverse",
            ["UPPER"] = "Upper",
            ["LEN"] = "Len",
            ["LOWER"] = "Lower",
            ["RTRIM"] = "RTrim",
            ["BINARY_CHECKSUM"] = "Binary_Checksum",
            ["HOST_NAME"] = "Host_Name",
            ["CHECKSUM"] = "Checksum",
            ["ISNULL"] = "IsNull",
            ["CONNECTIONPROPERTY"] = "ConnectionProperty",
            ["ISNUMERIC"] = "IsNumeric",
            ["CONTEXT_INFO"] = "Context_Info",
            ["MIN_ACTIVE_ROWVERSION"] = "Min_Active_Rowversion",
            ["CURRENT_REQUEST_ID"] = "Current_Request_Id",
            ["NEWID"] = "NewId",
            ["ERROR_LINE"] = "Error_Line",
            ["NEWSEQUENTIALID"] = "NewSquentialId",
            ["ERROR_MESSAGE"] = "Error_Messsge",
            ["ROWCOUNT_BIG"] = "RowCount_Big",
            ["ERROR_NUMBER"] = "Error_Number",
            ["XACT_STATE"] = "XAct_State",
            ["ERROR_PROCEDURE"] = "Error_Procedure",
            ["HOST_ID"] = "Host_Id",
            ["ERROR_SEVERITY"] = "Error_Severity",
            ["ERROR_STATE"] = "Error_State",
            ["FORMATMESSAGE"] = "FormatMesssage",
            ["GETANSINULL"] = "GetAnIsNull",
            ["TEXTVALID"] = "TextValid",
            ["TEXTPTR"] = "TextPTR",
            ["COALESCE"] = "Coalesce",
            ["COLLATE"] = "Collate",
            ["SESSION_USER"] = "Session_User",
            ["CONTAINS"] = "Contains",
            ["SYSTEM_USER"] = "System_User",
            ["CURRENT_TIME"] = "Current_Time",
            ["CURRENT_USER"] = "Current_User",
            ["NULLIF"] = "NullIf",
            ["TSEQUAL"] = "TSqueal",
            ["EXTRACT"] = "Extract",
            ["BIT_LENGTH"] = "Bit_Length",
            ["HOUR"] = "Hour",
            ["SECOND"] = "Second",
            ["MINUTE"] = "Minute",
            ["OCTET_LENGTH"] = "Octet_Length",
            ["DAYOFYEAR"] = "DayOfYear"
        };

        internal static Dictionary<string, string> SqlOperatorMap = new Dictionary<string, string>()
        {
            ["ALL"] = "All",
            ["AND"] = "And",
            ["ANY"] = "Any",
            ["BETWEEN"] = "Between",
            ["RIGHT"] = "Right",
            ["IN"] = "In",
            ["INNER"] = "Inner",
            ["IS"] = "Is",
            ["JOIN"] = "Join",
            ["SOME"] = "Some",
            ["LEFT"] = "Left",
            ["CROSS"] = "Cross",
            ["NOT"] = "Not",
            ["NULL"] = "Null",
            ["UNPIVOT"] = "Unpivot",
            ["OR"] = "Or",
            ["OUTER"] = "Outer",
            ["PIVOT"] = "Pivot",
            ["EXISTS"] = "Exists",
            ["LIKE"] = "Like"
        };
    }
}
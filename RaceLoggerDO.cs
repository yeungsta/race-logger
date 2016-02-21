using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;

namespace RaceLogger
{
    public class RaceType
    {
        public const string FiveK = "5k";
        public const string AquaRun = "AquaRun (Individual)";
        public const string AquaRunTeam = "AquaRun (Team)";
    }

    public class TeamComponent
    {
        public const string Swimmer = "swimmer";
        public const string Runner = "runner";
    }

    public class AgeGroup
    {
        public const string Group1 = "14 & Under";
        public const string Group2 = "15 - 19";
        public const string Group3 = "20 - 29";
        public const string Group4 = "30 - 39";
        public const string Group5 = "40 - 49";
        public const string Group6 = "50 - 59";
        public const string Group7 = "60 - 69";
        public const string Group8 = "70+";
    }

    public class HeatGroup
    {
        public const string Heat1 = "1";
        public const string Heat2 = "2";
        public const string Heat3 = "3";
    }

    public class BestTimeType
    {
        public const string BestTime5k = "BestTime5k";
        public const string BestTimeAquaRun = "BestTimeAquaRun";
        public const string BestTimeAquaRunTeam = "BestTimeAquaRunTeam";
    }

    public class DataFields
    {
        //don't allow spaces cause used in data table filtering
        public const string FirstName = "FirstName";
        public const string LastName = "LastName";
        public const string BIB = "BIB";
        public const string RFID = "RFID";
        public const string RaceType = "RaceType";
        public const string TeamComponent = "TeamComponent";
        public const string Heat = "Heat";
        public const string AgeGroup = "AgeGroup";
        public const string SwimStartTime = "SwimStartTime";
        public const string RunStartTime = "RunStartTime";
        public const string EndTime = "EndTime";
        public const string TotalDuration = "TotalDuration";
        public const string RunDuration = "RunDuration";
    }

    public class LogModeType
    {
        public const string LogStartTime = "Log Start Times";
        public const string LogEndTime = "Log End Times";
    }

    public class RaceLoggerDO
    {
        private const string TableXML = "lastTable.xml";
        private DataSet m_raceLoggerDS;
        private DataTable m_raceLoggerDT;
        private DataColumn m_colFirstName;
        private DataColumn m_colLastName;
        private DataColumn m_colBIB;
        private DataColumn m_colRFID;
        private DataColumn m_colRace;
        private DataColumn m_colTeamComp;
        private DataColumn m_colHeat;
        private DataColumn m_colAgeGroup;
        private DataColumn m_colSwimStartTime;
        private DataColumn m_colRunStartTime;
        private DataColumn m_colEndTime;
        private DataColumn m_colTotalDuration;
        private DataColumn m_colRunDuration;
        private string m_logMode;

        public DataSet DataSet
        {
            get { return m_raceLoggerDS; }
            set { m_raceLoggerDS = value; }
        }

        public DataTable DataTable
        {
            get { return m_raceLoggerDT; }
            set { m_raceLoggerDT = value; }
        }

        public string LogMode
        {
            get { return m_logMode; }
            set { m_logMode = value; }
        }

        public RaceLoggerDO()
        {
            m_raceLoggerDS = new DataSet();
            m_raceLoggerDT = new DataTable();
            m_colFirstName = new DataColumn();
            m_colLastName = new DataColumn();
            m_colBIB = new DataColumn();
            m_colRFID = new DataColumn();
            m_colRace = new DataColumn();
            m_colTeamComp = new DataColumn();
            m_colHeat = new DataColumn();
            m_colAgeGroup = new DataColumn();
            m_colSwimStartTime = new DataColumn();
            m_colRunStartTime = new DataColumn();
            m_colEndTime = new DataColumn();
            m_colTotalDuration = new DataColumn();
            m_colRunDuration = new DataColumn();
            m_logMode = LogModeType.LogStartTime;

            m_raceLoggerDS.DataSetName = "raceLoggerDS";
            m_raceLoggerDS.Tables.AddRange(new DataTable[] {
                m_raceLoggerDT
            });

            m_raceLoggerDT.Columns.AddRange(new System.Data.DataColumn[] {
                m_colFirstName,
                m_colLastName,
                m_colBIB,
                m_colRFID,
                m_colRace,
                m_colTeamComp,
                m_colHeat,
                m_colAgeGroup,
                m_colSwimStartTime,
                m_colRunStartTime,
                m_colEndTime,
                m_colTotalDuration,
                m_colRunDuration
            });
            m_raceLoggerDT.TableName = "raceLoggerDT";

            m_colFirstName.ColumnName = DataFields.FirstName;
            m_colLastName.ColumnName = DataFields.LastName;
            m_colBIB.ColumnName = DataFields.BIB;
            m_colRFID.ColumnName = DataFields.RFID;
            m_colRace.ColumnName = DataFields.RaceType;
            m_colTeamComp.ColumnName = DataFields.TeamComponent;
            m_colHeat.ColumnName = DataFields.Heat;
            m_colAgeGroup.ColumnName = DataFields.AgeGroup;
            m_colSwimStartTime.ColumnName = DataFields.SwimStartTime;
            m_colRunStartTime.ColumnName = DataFields.RunStartTime;
            m_colEndTime.ColumnName = DataFields.EndTime;
            m_colTotalDuration.ColumnName = DataFields.TotalDuration;
            m_colRunDuration.ColumnName = DataFields.RunDuration;

            ReadInCurrentXML();
        }

        public void ReadInCurrentXML()
        {
            if (System.IO.File.Exists(TableXML)) 
            {
                m_raceLoggerDT.ReadXml(TableXML);
            }
        }

        public void SaveToXML()
        {
            try
            {
                m_raceLoggerDT.WriteXml(TableXML, true);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Exception: " + e.Message);
            }
        }

        public void CalculateRunDuration()
        {
            for (int i = 0; i < m_raceLoggerDT.Rows.Count; i++)
            {
                if (!(m_raceLoggerDT.Rows[i][DataFields.RunStartTime] is System.DBNull) &&
                    (m_raceLoggerDT.Rows[i][DataFields.RunStartTime] != null) &&
                    !(m_raceLoggerDT.Rows[i][DataFields.EndTime] is System.DBNull) &&
                    (m_raceLoggerDT.Rows[i][DataFields.EndTime] != null))
                {
                    try
                    {
                        DateTime startTime = DateTime.Parse(m_raceLoggerDT.Rows[i][DataFields.RunStartTime] as string);
                        DateTime endTime = DateTime.Parse(m_raceLoggerDT.Rows[i][DataFields.EndTime] as string);

                        if (startTime != null && endTime != null)
                        {
                            TimeSpan duration = endTime - startTime;
                            m_raceLoggerDT.Rows[i][DataFields.RunDuration] = duration.ToString();

                        }
                    }
                    catch (Exception e)
                    {
                        //Do nothing; just skip this row's calculation.
                    }
                }
            }

            SaveToXML();
        }

        public void CalculateTotalDuration()
        {
            for (int i = 0; i < m_raceLoggerDT.Rows.Count; i++)
            {
                //If this is a team entry
                if ((string)m_raceLoggerDT.Rows[i][DataFields.RaceType] == RaceType.AquaRunTeam)
                {
                    DateTime startTime;
                    DateTime endTime;

                    //First entry is a swimmer
                    if ((string)m_raceLoggerDT.Rows[i][DataFields.TeamComponent] == TeamComponent.Swimmer)
                    {
                        if (!(m_raceLoggerDT.Rows[i][DataFields.SwimStartTime] is System.DBNull) &&
                        (m_raceLoggerDT.Rows[i][DataFields.SwimStartTime] != null))
                        {
                            try
                            {
                                startTime = DateTime.Parse(m_raceLoggerDT.Rows[i][DataFields.SwimStartTime] as string);

                                //increment to next entry
                                i++;
                                if ((string)m_raceLoggerDT.Rows[i][DataFields.TeamComponent] == TeamComponent.Runner)
                                {
                                    if (!(m_raceLoggerDT.Rows[i][DataFields.EndTime] is System.DBNull) &&
                                        (m_raceLoggerDT.Rows[i][DataFields.EndTime] != null))
                                    {
                                        endTime = DateTime.Parse(m_raceLoggerDT.Rows[i][DataFields.EndTime] as string);

                                        if (startTime != null && endTime != null)
                                        {
                                            TimeSpan duration = endTime - startTime;
                                            m_raceLoggerDT.Rows[i][DataFields.TotalDuration] = duration.ToString();
                                        }
                                    }
                                }
                                else
                                {
                                    System.Windows.Forms.MessageBox.Show("A team has a mismatched team component!");
                                }
                            }
                            catch (Exception e)
                            {
                                //Do nothing; just skip this row's calculation.
                            }
                        }
                        else
                        {
                            //increment to next entry
                            i++;
                        }
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("A team has a mismatched team component!");
                    }
                }
                else
                {
                    if (!(m_raceLoggerDT.Rows[i][DataFields.SwimStartTime] is System.DBNull) &&
                        (m_raceLoggerDT.Rows[i][DataFields.SwimStartTime] != null) &&
                        !(m_raceLoggerDT.Rows[i][DataFields.EndTime] is System.DBNull) &&
                        (m_raceLoggerDT.Rows[i][DataFields.EndTime] != null))
                    {
                        try
                        {
                            DateTime startTime = DateTime.Parse(m_raceLoggerDT.Rows[i][DataFields.SwimStartTime] as string);
                            DateTime endTime = DateTime.Parse(m_raceLoggerDT.Rows[i][DataFields.EndTime] as string);

                            if (startTime != null && endTime != null)
                            {
                                TimeSpan duration = endTime - startTime;
                                m_raceLoggerDT.Rows[i][DataFields.TotalDuration] = duration.ToString();
                            }
                        }
                        catch (Exception e)
                        {
                            //Do nothing; just skip this row's calculation.
                        }
                    }
                }
            }

            SaveToXML();
        }

        public void ClearStartTimes()
        {
            for (int i = 0; i < m_raceLoggerDT.Rows.Count; i++)
            {
                m_raceLoggerDT.Rows[i][DataFields.SwimStartTime] = null;
                m_raceLoggerDT.Rows[i][DataFields.RunStartTime] = null;
            }

            SaveToXML();
        }

        public void ClearEndTimes()
        {
            for (int i = 0; i < m_raceLoggerDT.Rows.Count; i++)
            {
                m_raceLoggerDT.Rows[i][DataFields.EndTime] = null;
            }

            SaveToXML();
        }

        public void ClearDurationTimes()
        {
            for (int i = 0; i < m_raceLoggerDT.Rows.Count; i++)
            {
                m_raceLoggerDT.Rows[i][DataFields.TotalDuration] = null;
                m_raceLoggerDT.Rows[i][DataFields.RunDuration] = null;
            }

            SaveToXML();
        }

        public void ReplaceDataTable(DataTable newTable)
        {
            m_raceLoggerDT.Clear();
            m_raceLoggerDT.Merge(newTable);
            SaveToXML(); 
        }

        public void ImportStartTimes(DataTable newTable)
        {
            for (int i = 0; i < m_raceLoggerDT.Rows.Count; i++)
            {
                m_raceLoggerDT.Rows[i][DataFields.SwimStartTime] = newTable.Rows[i][DataFields.SwimStartTime];
                m_raceLoggerDT.Rows[i][DataFields.RunStartTime] = newTable.Rows[i][DataFields.RunStartTime];
            }

            SaveToXML();
        }

        public void ImportEndTimes(DataTable newTable)
        {
            for (int i = 0; i < m_raceLoggerDT.Rows.Count; i++)
            {
                m_raceLoggerDT.Rows[i][DataFields.EndTime] = newTable.Rows[i][DataFields.EndTime];
            }

            SaveToXML();
        }

        public DataTable ParseCSVFile(string path)
        {

            string inputString = "";

            // check that the file exists before opening it
            if (System.IO.File.Exists(path))
            {
                System.IO.StreamReader sr = new System.IO.StreamReader(path);
                inputString = sr.ReadToEnd();
                sr.Close();
            }

            return ParseCSV(inputString);
        }

        // This method is used to Convert DataTable to CSV ( comma seperated ) file.
        public void ExportToCSV(DataTable table, string filename, string seperateChar)
        {

            StreamWriter sr = null;

            try
            {

                sr = new StreamWriter(filename, false);
                string seperator = "";
                StringBuilder builder = new StringBuilder();
                foreach (DataColumn col in table.Columns)
                {

                    builder.Append(seperator).Append(col.ColumnName);

                    seperator = seperateChar;
                }

                sr.WriteLine(builder.ToString());

                foreach (DataRow row in table.Rows)
                {

                    seperator = "";
                    builder = new StringBuilder();
                    foreach (DataColumn col in table.Columns)
                    {

                        builder.Append(seperator).Append(row[col.ColumnName]);
                        seperator = seperateChar;

                    }

                    sr.WriteLine(builder.ToString());

                }

            }

            finally
            {

                if (sr != null)
                {

                    sr.Close();

                }

            }
        }

        public void SetCurrentViewSwimStartTime()
        {
            for (int i = 0; i < m_raceLoggerDT.DefaultView.Count; i++)
            {
                if ((m_raceLoggerDT.DefaultView[i][DataFields.SwimStartTime] == null) ||
                (m_raceLoggerDT.DefaultView[i][DataFields.SwimStartTime] is System.DBNull))
                {
                    m_raceLoggerDT.DefaultView[i][DataFields.SwimStartTime] = DateTime.Now.ToLocalTime().ToLongTimeString().ToString();
                }
            }

            SaveToXML();
        }

        #region Private methods

        private DataTable ParseCSV(string inputString)
        {
            DataTable dt = new DataTable();

            // declare the Regular Expression that will match versus the input string
            Regex re = new Regex("((?<field>[^\",\\r\\n]+)|\"(?<field>([^\"]|\"\")+)\")(,|(?<rowbreak>\\r\\n|\\n|$))");

            ArrayList colArray = new ArrayList();
            ArrayList rowArray = new ArrayList();

            int colCount = 0;
            int maxColCount = 0;
            string rowbreak = "";
            string field = "";

            MatchCollection mc = re.Matches(inputString);

            foreach (Match m in mc)
            {

                // retrieve the field and replace two double-quotes with a single double-quote
                field = m.Result("${field}").Replace("\"\"", "\"");

                rowbreak = m.Result("${rowbreak}");

                if (field.Length > 0)
                {
                    colArray.Add(field);
                    colCount++;
                }

                if (rowbreak.Length > 0)
                {

                    // add the column array to the row Array List
                    rowArray.Add(colArray.ToArray());

                    // create a new Array List to hold the field values
                    colArray = new ArrayList();

                    if (colCount > maxColCount)
                        maxColCount = colCount;

                    colCount = 0;
                }
            }

            if (rowbreak.Length == 0)
            {
                // this is executed when the last line doesn't
                // end with a line break
                rowArray.Add(colArray.ToArray());
                if (colCount > maxColCount)
                    maxColCount = colCount;
            }

            // convert the row Array List into an Array object for easier access
            Array ra = rowArray.ToArray();

            // Create the columns for the table

            // get first row (column header row)
            //Array headerRow = (Array)(ra.GetValue(0));

            // add each column field to the table
            dt.Columns.Add(DataFields.FirstName);
            dt.Columns.Add(DataFields.LastName);
            dt.Columns.Add(DataFields.BIB);
            dt.Columns.Add(DataFields.RFID);
            dt.Columns.Add(DataFields.RaceType);
            dt.Columns.Add(DataFields.TeamComponent);
            dt.Columns.Add(DataFields.Heat);
            dt.Columns.Add(DataFields.AgeGroup);
            dt.Columns.Add(DataFields.SwimStartTime);
            dt.Columns.Add(DataFields.RunStartTime);
            dt.Columns.Add(DataFields.EndTime);
            dt.Columns.Add(DataFields.TotalDuration);
            dt.Columns.Add(DataFields.RunDuration);

            //skip first row (column headers)
            for (int i = 1; i < ra.Length; i++)
            {

                // create a new DataRow
                DataRow dr = dt.NewRow();

                // convert the column Array List into an Array object for easier access
                Array ca = (Array)(ra.GetValue(i));

                // add each field into the new DataRow
                for (int j = 0; j < ca.Length; j++)
                    dr[j] = ca.GetValue(j);

                // add the new DataRow to the DataTable
                dt.Rows.Add(dr);
            }

            // in case no data was parsed, create a single column
            if (dt.Columns.Count == 0)
                dt.Columns.Add("NoData");

            return dt;
        }

        #endregion Private methods
    }
}

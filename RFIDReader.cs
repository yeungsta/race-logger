using Phidgets;
using Phidgets.Events;
using System;
using System.Windows.Forms;
using System.Data;

namespace RaceLogger
{
    public class RFIDReader
    {
        private RFID m_rfid;
        private int m_serial;
        private RaceLoggerDO m_dataObject;
        private RaceLoggerGUI m_gui;

/*
        public event AttachEventHandler Attach;
        public event DetachEventHandler Detach;
        public event ErrorEventHandler Error;
        public event TagEventHandler TagFound;
        public event TagEventHandler TagLost;
 */ 

        public int Serial
        {
            get { return m_serial; }
        }

        public RFIDReader(RaceLoggerDO dataObject, RaceLoggerGUI gui)
        {
            m_dataObject = dataObject;
            m_gui = gui;
            m_serial = 0;

            PassRefToGUI();
        }

        public void Initialize()
        {
            try
            {
                //Declare an RFID object
                m_rfid = new RFID(); 

                //initialize our Phidgets RFID reader and hook the event handlers
                m_rfid.Attach += new AttachEventHandler(RFID_Attached);
                m_rfid.Detach += new DetachEventHandler(RFID_Detached);
                m_rfid.Error += new ErrorEventHandler(RFID_Error);
                m_rfid.Tag += new TagEventHandler(RFID_Tag_Found);
                m_rfid.TagLost += new TagEventHandler(RFID_Tag_Lost);

                //Open RFID
                m_rfid.open();
            }
            catch (PhidgetException ex)
            {
                MessageBox.Show(ex.Description);
            }
        }

        public void Shutdown()
        {
            try
            {
                if (m_rfid != null)
                {
                    //close the phidget and dispose of the object
                    m_rfid.close();
                    m_rfid = null;
                }
            }
            catch (PhidgetException ex)
            {
                MessageBox.Show(ex.Description);
            }
        }

        //attach event handler...display the serial number of the attached RFID phidget
        private void RFID_Attached(object sender, AttachEventArgs e)
        {
            m_rfid.Antenna = true;
            m_serial = e.Device.SerialNumber;
            TurnGUIButtonOn();
            //Attach(sender, e);
        }

        //detach event handler...display the serial number of the detached RFID phidget
        private void RFID_Detached(object sender, DetachEventArgs e)
        {
            m_serial = 0;
            TurnGUIButtonOff();
            //Detach(sender, e);
        }

        //Error event handler...display the error description string
        private void RFID_Error(object sender, ErrorEventArgs e)
        {
            MessageBox.Show("Error: " + e.Description);
            //Error(sender, e);
        }

        //Print the tag code of the scanned tag
        private void RFID_Tag_Found(object sender, TagEventArgs e)
        {
            //turn on buzzer
            m_rfid.outputs[0] = true;

            DataView dataView = new DataView(m_dataObject.DataTable);
            dataView.RowFilter = DataFields.RFID + " = '" + e.Tag + "'";
            if (dataView.Count == 0)
            {
                //Entry does not exist yet
                string timeStamp = DateTime.Now.ToLocalTime().ToLongTimeString().ToString();
                m_dataObject.DataTable.Rows.Add(string.Empty, string.Empty, e.Tag.ToString(), string.Empty, string.Empty, timeStamp, string.Empty, string.Empty);
                m_dataObject.DataTable.AcceptChanges();
                m_dataObject.SaveToXML();

                //Display
                m_gui.DisplayInReadingLabel("Participant not found! Recorded new entry: " + timeStamp);   
            }
            else if (dataView.Count == 1)
            {
                string startTime = null;
                string endTime = null;

                if (!(dataView[0][DataFields.RunStartTime] is System.DBNull) &&
                    (dataView[0][DataFields.RunStartTime] != null))
                {
                    startTime = ((string)dataView[0][DataFields.RunStartTime]).Trim();
                }

                if (!(dataView[0][DataFields.EndTime] is System.DBNull) &&
                    (dataView[0][DataFields.EndTime] != null))
                {
                    endTime = ((string)dataView[0][DataFields.EndTime]).Trim();
                }

                //Log start time if log mode is start
                if ((m_dataObject.LogMode == LogModeType.LogStartTime) &&
                    (string.IsNullOrEmpty(startTime)))
                {
                    string timeStamp = DateTime.Now.ToLocalTime().ToLongTimeString().ToString();

                    dataView[0].BeginEdit();
                    dataView[0][DataFields.RunStartTime] = timeStamp;
                    dataView[0].EndEdit();
                    m_dataObject.SaveToXML();

                    //Display
                    m_gui.DisplayInReadingLabel(((string)dataView[0][DataFields.FirstName]) +
                        " " +
                        ((string)dataView[0][DataFields.LastName]) +
                        " (" + 
                        dataView[0][DataFields.BIB] +
                        ") - " +
                        timeStamp +
                        "\nStart Run Time logged successfully.");   
                }
                //Else log end time if log mode is end time
                else if ((m_dataObject.LogMode == LogModeType.LogEndTime) &&
                    (string.IsNullOrEmpty(endTime)))
                {
                    string timeStamp = DateTime.Now.ToLocalTime().ToLongTimeString().ToString();

                    dataView[0].BeginEdit();
                    dataView[0][DataFields.EndTime] = timeStamp;
                    dataView[0].EndEdit();
                    m_dataObject.SaveToXML();

                    //Display
                    m_gui.DisplayInReadingLabel(((string)dataView[0][DataFields.FirstName]) +
                        " " +
                        ((string)dataView[0][DataFields.LastName]) +
                        " (" +
                        dataView[0][DataFields.BIB] +
                        ") - " +
                        timeStamp +
                        "\nEnd Time logged successfully.");   
                }
            }
            //should not find more than one
            else if (dataView.Count > 1)
            {
                MessageBox.Show("Error: Duplicate RFID found in database!");
            }

            //TagFound(sender, e);
        }

        //print the tag code for the tag that was just lost
        private void RFID_Tag_Lost(object sender, TagEventArgs e)
        {
            //turn off buzzer
            m_rfid.outputs[0] = false;
        }

        private void PassRefToGUI()
        {
            //set this class reference back to the GUI so it can turn the
            //RFID reader on/off. Try to set to either RFID reader 1, or 
            //if already set, 2.
            if (m_gui.RFIDReader1 == null)
            {
                m_gui.RFIDReader1 = this;
            }
            else if (m_gui.RFIDReader2 == null)
            {
                m_gui.RFIDReader2 = this;
            }
        }

        private void TurnGUIButtonOn()
        {
            if (m_gui.RFIDReader1 == this)
            {
                m_gui.RFID1ButtonOn(m_serial);
            }
            else if (m_gui.RFIDReader2 == this)
            {
                m_gui.RFID2ButtonOn(m_serial);
            }
        }

        private void TurnGUIButtonOff()
        {
            if (m_gui.RFIDReader1 == this)
            {
                m_gui.RFID1ButtonOff();
            }
            else if (m_gui.RFIDReader2 == this)
            {
                m_gui.RFID2ButtonOff();
            }
        }
    }
}

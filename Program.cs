using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RaceLogger
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            RFIDReader m_rfidReader1;
            RFIDReader m_rfidReader2;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            RaceLoggerDO dataObject = new RaceLoggerDO();

            RaceLoggerGUI raceLoggerGUI = new RaceLoggerGUI(dataObject);

            m_rfidReader1 = new RFIDReader(dataObject, raceLoggerGUI);
            m_rfidReader2 = new RFIDReader(dataObject, raceLoggerGUI);

            Application.Run(raceLoggerGUI);
        }
    }
}

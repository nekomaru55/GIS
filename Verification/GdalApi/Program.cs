using GdalApi.View.Forms;
using log4net;
using OSGeo.GDAL;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace GdalApi {
    static class Program {
        #region --- [ Logger ] ---
        private static ILog m_ctrlLogger = LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        #region --- [ Defines ] ---
        private static readonly string FILE_IMG_TEST_B1 = "test_b1.tif";
        #endregion
        [STAThread]
        static void Main() {
            m_ctrlLogger.Info("START APPLICATION.");
            try {
                //============================================================
                // GDAL initialize
                //============================================================
                Gdal.AllRegister();

                //============================================================
                // GeoTIFF file copy
                //============================================================
                if (File.Exists(FILE_IMG_TEST_B1) == false) {
                    new Bitmap(Properties.Resources.test_b1).Save(FILE_IMG_TEST_B1);
                    m_ctrlLogger.InfoFormat("Save GeoTIFF [{0}]", 
                        Path.GetFullPath(FILE_IMG_TEST_B1));
                }

                //============================================================
                // Application setup
                //============================================================
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());

            } catch (Exception ex) {
                m_ctrlLogger.Fatal(ex);
            } finally {
                m_ctrlLogger.Info("END APPLICATION.");
            }
        }
    }
}

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
        public static readonly string[] FILE_LANDSAT_OUT_IMG =
            new string[] { "test_b1.tif", "test_b2.tif", "test_b3.tif", "test_b4.tif",
                            "test_b5.tif","test_b6.tif","test_b7.tif"   };
        private static readonly Bitmap[] FILE_LANDSAT_IN_IMG =
            new Bitmap[] { Properties.Resources.test_b1,
                Properties.Resources.test_b2,
                Properties.Resources.test_b3,
                Properties.Resources.test_b4,
                Properties.Resources.test_b5,
                Properties.Resources.test_b6,
                Properties.Resources.test_b7};
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
                for (int i = 0; i < FILE_LANDSAT_OUT_IMG.Length; i++) {
                    if (File.Exists(FILE_LANDSAT_OUT_IMG[i]) == false) {
                        new Bitmap(FILE_LANDSAT_IN_IMG[i]).Save(FILE_LANDSAT_OUT_IMG[i]);
                    }
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

using log4net;
using OSGeo.GDAL;
using OSGeo.OSR;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace GdalApi.Model.Domain.Object {
    public class GeoTiffObject {
        #region --- [ Logger ] ---
        static ILog m_ctrlLogger = LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        #region --- [ Fields ] ---
        private Bitmap m_tBitmap;
        private Dataset m_tGdalDs;
        private Driver m_tGdalDriver;
        private string m_strGeoTiffPath;
        private StringBuilder m_strGdalInfoTxt = new StringBuilder();
        #endregion
        #region --- [ Propertys ] ---
        public Dataset gdalDataset {
            get {
                return m_tGdalDs;
            }
        }
        public Driver gdalDriver {
            get {
                return m_tGdalDriver;
            }
        }
        public string gdalInfoText {
            get {
                return m_strGdalInfoTxt.ToString();
            }
        }
        public Bitmap Image {
            get {
                return m_tBitmap;
            }
        }
        #endregion
        #region --- [ Constructor ] ---
        public GeoTiffObject(string strGeoTiffPath) {
            //================================================================
            // Initialize
            //================================================================
            m_strGeoTiffPath = Path.GetFullPath(strGeoTiffPath);

            //================================================================
            // Error check
            //================================================================
            if (string.IsNullOrEmpty(strGeoTiffPath)) {
                throw new InvalidOperationException("GeoTIFF file is empty.");
            }
            if (File.Exists(strGeoTiffPath) == false) {
                throw new InvalidOperationException("File does not exist.");
            }

            //================================================================
            // Open GeoTiff
            //================================================================
            getGdalinfo();

            //================================================================
            // Create bitmap
            //================================================================
            getBitmapBuffered(-1);
        }
        #endregion
        #region --- [ Public API ] ---
        public void reload(string strGeoTiffPath = "") {
            if (string.IsNullOrEmpty(strGeoTiffPath) == false) {
                m_strGeoTiffPath = Path.GetFullPath(strGeoTiffPath);
                //------------------------------------------------------------
                // Error check
                //------------------------------------------------------------
                if (string.IsNullOrEmpty(strGeoTiffPath)) {
                    throw new InvalidOperationException("GeoTIFF file is empty.");
                }
                if (File.Exists(strGeoTiffPath) == false) {
                    throw new InvalidOperationException("File does not exist.");
                }
            }
            //================================================================
            // Open GeoTiff
            //================================================================
            getGdalinfo();

            //================================================================
            // Create bitmap
            //================================================================
            getBitmapBuffered(-1);
        }
        public void save(string strSaveFilePath) {
            try {
                m_tBitmap.Save(strSaveFilePath);
                m_ctrlLogger.InfoFormat("Save bitmap file.[{0}]", strSaveFilePath);
            } catch (ArgumentException ex) {
                m_ctrlLogger.Error(ex);
            } catch (ExternalException ex) {
                m_ctrlLogger.Error(ex);
            }
        }
        #endregion
        #region --- [ Methods ] ---
        private void getGdalinfo() {
            //================================================================
            // Initialize
            //================================================================
            reset();

            //================================================================
            // Get gdal info
            //================================================================
            try {
                //------------------------------------------------------------
                // Open dataset
                //------------------------------------------------------------
                m_tGdalDs = Gdal.Open(m_strGeoTiffPath, Access.GA_ReadOnly);

                if (m_tGdalDs == null) {
                    m_ctrlLogger.FatalFormat("Can't open {0}", m_strGeoTiffPath);
                    throw new ArgumentException("Parameter cannot be null", "original");
                }
                m_strGdalInfoTxt.AppendLine("GeoTIFF file path:");
                m_strGdalInfoTxt.AppendFormat("  {0}", m_strGeoTiffPath).AppendLine();
                m_strGdalInfoTxt.AppendLine("Raster dataset parameters:");
                m_strGdalInfoTxt.AppendFormat("  Projection: {0}",
                    m_tGdalDs.GetProjectionRef()).AppendLine();
                m_strGdalInfoTxt.AppendFormat("  RasterCount: {0}",
                    m_tGdalDs.RasterCount).AppendLine();
                m_strGdalInfoTxt.AppendFormat("  RasterSize ({0},{1})",
                    m_tGdalDs.RasterXSize, m_tGdalDs.RasterYSize).AppendLine();

                //------------------------------------------------------------
                // Get driver
                //------------------------------------------------------------
                m_tGdalDriver = m_tGdalDs.GetDriver();

                if (m_tGdalDriver == null) {
                    m_ctrlLogger.Fatal("Can't get driver.");
                    throw new ArgumentException("Parameter cannot be null", "original");
                }

                m_strGdalInfoTxt.AppendFormat("Using driver {0}",
                    m_tGdalDriver.LongName).AppendLine();

                //------------------------------------------------------------
                // Get meta data
                //------------------------------------------------------------
                string[] metadata = m_tGdalDs.GetMetadata(string.Empty);
                if (metadata.Length > 0) {
                    m_strGdalInfoTxt.AppendLine("  Metadata:");
                    for (int iMeta = 0; iMeta < metadata.Length; iMeta++) {
                        m_strGdalInfoTxt.AppendFormat("    {0}: {1}",
                            iMeta, metadata[iMeta]).AppendLine();
                    }
                    m_strGdalInfoTxt.AppendLine();
                }

                //------------------------------------------------------------
                // Report "IMAGE_STRUCTURE" metadata
                //------------------------------------------------------------
                metadata = m_tGdalDs.GetMetadata("IMAGE_STRUCTURE");
                if (metadata.Length > 0) {
                    m_strGdalInfoTxt.AppendLine("  Image Structure Metadata:");
                    for (int iMeta = 0; iMeta < metadata.Length; iMeta++) {
                        m_strGdalInfoTxt.AppendFormat("    {0}: {1}",
                            iMeta, metadata[iMeta]).AppendLine();
                    }
                    m_strGdalInfoTxt.AppendLine();
                }

                //------------------------------------------------------------
                // Report subdatasets
                //------------------------------------------------------------
                metadata = m_tGdalDs.GetMetadata("SUBDATASETS");
                if (metadata.Length > 0) {
                    m_strGdalInfoTxt.AppendLine("  Subdatasets:");
                    for (int iMeta = 0; iMeta < metadata.Length; iMeta++) {
                        m_strGdalInfoTxt.AppendFormat("    {0}: {1}",
                            iMeta, metadata[iMeta]).AppendLine();
                    }
                    m_strGdalInfoTxt.AppendLine();
                }

                //------------------------------------------------------------
                // Report geolocation
                //------------------------------------------------------------
                metadata = m_tGdalDs.GetMetadata("GEOLOCATION");
                if (metadata.Length > 0) {
                    m_strGdalInfoTxt.AppendLine("  Geolocation:");
                    for (int iMeta = 0; iMeta < metadata.Length; iMeta++) {
                        m_strGdalInfoTxt.AppendFormat("    {0}: {1}",
                            iMeta, metadata[iMeta]).AppendLine();
                    }
                    m_strGdalInfoTxt.AppendLine();
                }

                //------------------------------------------------------------
                // Report corners
                //------------------------------------------------------------
                m_strGdalInfoTxt.AppendLine("Corner Coordinates:");
                m_strGdalInfoTxt.AppendFormat("  Upper Left ({0})",
                    GDALInfoGetPosition(0.0, 0.0)).AppendLine();
                m_strGdalInfoTxt.AppendFormat("  Lower Left ({0})",
                    GDALInfoGetPosition(0.0, m_tGdalDs.RasterYSize)).AppendLine();
                m_strGdalInfoTxt.AppendFormat("  Upper Right ({0})",
                    GDALInfoGetPosition(m_tGdalDs.RasterXSize, 0.0)).AppendLine();
                m_strGdalInfoTxt.AppendFormat("  Lower Right ({0})",
                    GDALInfoGetPosition(m_tGdalDs.RasterXSize, m_tGdalDs.RasterYSize)).AppendLine();
                m_strGdalInfoTxt.AppendFormat("  Center ({0})",
                    GDALInfoGetPosition(m_tGdalDs.RasterXSize / 2, m_tGdalDs.RasterYSize / 2)).AppendLine();
                m_strGdalInfoTxt.AppendLine();

                //------------------------------------------------------------
                // Report projection
                //------------------------------------------------------------
                string projection = m_tGdalDs.GetProjectionRef();
                if (projection != null) {
                    SpatialReference srs = new SpatialReference(null);
                    if (srs.ImportFromWkt(ref projection) == 0) {
                        string wkt;
                        srs.ExportToPrettyWkt(out wkt, 0);
                        m_strGdalInfoTxt.AppendLine("Coordinate System is:");
                        m_strGdalInfoTxt.AppendLine(wkt);
                    } else {
                        m_strGdalInfoTxt.AppendLine("Coordinate System is:");
                        m_strGdalInfoTxt.AppendLine(projection);
                    }
                }

                //------------------------------------------------------------
                // Report GCPs
                //------------------------------------------------------------
                if (m_tGdalDs.GetGCPCount() > 0) {
                    m_strGdalInfoTxt.AppendFormat("GCP Projection: {0}",
                        m_tGdalDs.GetGCPProjection()).AppendLine();
                    GCP[] GCPs = m_tGdalDs.GetGCPs();
                    for (int i = 0; i < m_tGdalDs.GetGCPCount(); i++) {
                        m_strGdalInfoTxt.AppendFormat("GCP[{0}]: Id={1}, Info={2}",
                            i, GCPs[i].Id, GCPs[i].Info).AppendLine();
                        m_strGdalInfoTxt.AppendFormat("          ({0},{1})->({2},{3},{4})",
                            GCPs[i].GCPPixel, GCPs[i].GCPLine, GCPs[i].GCPX, GCPs[i].GCPY, GCPs[i].GCPZ);
                        m_strGdalInfoTxt.AppendLine();
                    }
                    m_strGdalInfoTxt.AppendLine();

                    double[] transform = new double[6];
                    Gdal.GCPsToGeoTransform(GCPs, transform, 0);
                    m_strGdalInfoTxt.AppendFormat("GCP Equivalent geotransformation parameters: {0}",
                        m_tGdalDs.GetGCPProjection());
                    for (int i = 0; i < 6; i++) {
                        m_strGdalInfoTxt.AppendFormat("[{0}]= {1}", i, transform[i].ToString());
                    }
                    m_strGdalInfoTxt.AppendLine();
                }

                //------------------------------------------------------------
                // Get raster band
                //------------------------------------------------------------
                for (int iBand = 1; iBand <= m_tGdalDs.RasterCount; iBand++) {
                    Band band = m_tGdalDs.GetRasterBand(iBand);
                    m_strGdalInfoTxt.AppendFormat("Band {0} :",
                        iBand).AppendLine();
                    m_strGdalInfoTxt.AppendFormat("   DataType: {0}",
                        Gdal.GetDataTypeName(band.DataType)).AppendLine();
                    m_strGdalInfoTxt.AppendFormat("   ColorInterpretation: {0}",
                        Gdal.GetColorInterpretationName(band.GetRasterColorInterpretation())).AppendLine();
                    ColorTable ct = band.GetRasterColorTable();
                    if (ct != null) {
                        m_strGdalInfoTxt.AppendFormat("   Band has a color table with {0} entries.",
                            ct.GetCount()).AppendLine();
                    }

                    m_strGdalInfoTxt.AppendFormat("   Description: {0}",
                        band.GetDescription()).AppendLine();
                    m_strGdalInfoTxt.AppendFormat("   Size ({0},{1})",
                        band.XSize, band.YSize).AppendLine();
                    int BlockXSize, BlockYSize;
                    band.GetBlockSize(out BlockXSize, out BlockYSize);
                    m_strGdalInfoTxt.AppendFormat("   BlockSize ({0},{1})",
                        BlockXSize, BlockYSize).AppendLine();
                    double val;
                    int hasval;
                    band.GetMinimum(out val, out hasval);
                    if (hasval != 0) {
                        m_strGdalInfoTxt.AppendFormat("   Minimum: {0}",
                            val.ToString()).AppendLine();
                    }
                    band.GetMaximum(out val, out hasval);
                    if (hasval != 0) {
                        m_strGdalInfoTxt.AppendFormat("   Maximum: {0}",
                            val.ToString()).AppendLine();
                    }
                    band.GetNoDataValue(out val, out hasval);
                    if (hasval != 0) {
                        m_strGdalInfoTxt.AppendFormat("   NoDataValue: {0}",
                            val.ToString()).AppendLine();
                    }
                    band.GetOffset(out val, out hasval);
                    if (hasval != 0) {
                        m_strGdalInfoTxt.AppendFormat("   Offset: {0}",
                            val.ToString()).AppendLine();
                    }
                    band.GetScale(out val, out hasval);
                    if (hasval != 0) {
                        m_strGdalInfoTxt.AppendFormat("   Scale: {0}",
                            val.ToString()).AppendLine();
                    }

                    for (int iOver = 0; iOver < band.GetOverviewCount(); iOver++) {
                        Band over = band.GetOverview(iOver);
                        m_strGdalInfoTxt.AppendFormat("      OverView {0}:",
                            iOver).AppendLine();
                        m_strGdalInfoTxt.AppendFormat("         DataType: {0}",
                            over.DataType).AppendLine();
                        m_strGdalInfoTxt.AppendFormat("         Size ({0},{1})",
                            over.XSize, over.YSize).AppendLine();
                        m_strGdalInfoTxt.AppendFormat("         PaletteInterp: {0}",
                            over.GetRasterColorInterpretation().ToString()).AppendLine();
                    }
                }
            } catch (Exception ex) {
                m_ctrlLogger.FatalFormat("Application error: {0}", ex.Message);
            }
        }

        private string GDALInfoGetPosition(double x, double y) {
            double[] adfGeoTransform = new double[6];
            double dfGeoX, dfGeoY;
            m_tGdalDs.GetGeoTransform(adfGeoTransform);

            dfGeoX = adfGeoTransform[0] + adfGeoTransform[1] * x + adfGeoTransform[2] * y;
            dfGeoY = adfGeoTransform[3] + adfGeoTransform[4] * x + adfGeoTransform[5] * y;

            return dfGeoX.ToString() + ", " + dfGeoY.ToString();
        }

        private void reset() {
            if (m_tGdalDs != null) {
                m_tGdalDs.Dispose();
            }
            if (m_tGdalDriver != null) {
                m_tGdalDriver.Dispose();
            }
            if (m_tBitmap != null) {
                m_tBitmap.Dispose();
            }
            m_strGdalInfoTxt.Clear();
        }

        private void getBitmapBuffered(int iOverview) {
            //================================================================
            // Get the GDAL Band objects from the Dataset(Other)
            //================================================================
            if (m_tGdalDs.GetRasterBand(1).GetRasterColorInterpretation() ==
                ColorInterp.GCI_PaletteIndex) {
                getBitmapPaletteBuffered(iOverview);
                return;
            }
            if (m_tGdalDs.GetRasterBand(1).GetRasterColorInterpretation() ==
                ColorInterp.GCI_GrayIndex) {
                getBitmapGrayBuffered(iOverview);
                return;
            }

            //================================================================
            // Get the GDAL Band objects from the Dataset
            //================================================================
            Band redBand = getBandInfo(m_tGdalDs.GetRasterBand(1), iOverview,
                ColorInterp.GCI_RedBand);

            Band greenBand = getBandInfo(m_tGdalDs.GetRasterBand(2), iOverview,
                ColorInterp.GCI_GreenBand);

            Band blueBand = getBandInfo(m_tGdalDs.GetRasterBand(3), iOverview,
                ColorInterp.GCI_BlueBand);

            //================================================================
            // Get the width and height of the raster
            //================================================================
            int width = redBand.XSize;
            int height = redBand.YSize;

            //================================================================
            // Create a Bitmap to store the GDAL image in
            //================================================================
            m_tBitmap = new Bitmap(width, height, PixelFormat.Format32bppRgb);

            DateTime start = DateTime.Now;

            byte[] r = new byte[width * height];
            byte[] g = new byte[width * height];
            byte[] b = new byte[width * height];

            redBand.ReadRaster(0, 0, width, height, r, width, height, 0, 0);
            greenBand.ReadRaster(0, 0, width, height, g, width, height, 0, 0);
            blueBand.ReadRaster(0, 0, width, height, b, width, height, 0, 0);

            TimeSpan renderTime = DateTime.Now - start;
            m_ctrlLogger.DebugFormat("getBitmapBuffered fetch time: {0} ms",
                renderTime.TotalMilliseconds);

            //================================================================
            // Create bitmap(SetPixel)
            //================================================================
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    Color newColor = Color.FromArgb(Convert.ToInt32(r[i + j * width]),
                        Convert.ToInt32(g[i + j * width]), Convert.ToInt32(b[i + j * width]));
                    m_tBitmap.SetPixel(i, j, newColor);
                }
            }
        }

        private Band getBandInfo(Band tBand, int iOverview, ColorInterp type) {
            Band tRetBand = null;
            switch (type) {
                case ColorInterp.GCI_RedBand:
                    if (tBand.GetRasterColorInterpretation() != ColorInterp.GCI_RedBand) {
                        m_ctrlLogger.WarnFormat("Non RGB images are not supported by this sample! ColorInterp = {0}",
                            tBand.GetRasterColorInterpretation().ToString());
                        return tRetBand;
                    }

                    if (m_tGdalDs.RasterCount < 3) {
                        m_ctrlLogger.Warn("The number of the raster bands is not enough to run this sample");
                        return tRetBand;
                    }
                    break;
                case ColorInterp.GCI_BlueBand:
                    if (tBand.GetRasterColorInterpretation() != ColorInterp.GCI_BlueBand) {
                        m_ctrlLogger.WarnFormat("Non RGB images are not supported by this sample! ColorInterp = ",
                            tBand.GetRasterColorInterpretation().ToString());
                        return tRetBand;
                    }
                    break;
                case ColorInterp.GCI_GreenBand:
                    if (tBand.GetRasterColorInterpretation() != ColorInterp.GCI_GreenBand) {
                        m_ctrlLogger.WarnFormat("Non RGB images are not supported by this sample! ColorInterp = ",
                            tBand.GetRasterColorInterpretation().ToString());
                        return tRetBand;
                    }
                    break;
            }
            if (iOverview >= 0 && tBand.GetOverviewCount() > iOverview) {
                tRetBand = tBand.GetOverview(iOverview);
            } else {
                tRetBand = tBand;
            }
            return tRetBand;
        }

        private void getBitmapPaletteBuffered(int iOverview) {
            //================================================================
            // Get the GDAL Band objects from the Dataset
            //================================================================
            Band band = m_tGdalDs.GetRasterBand(1);
            if (iOverview >= 0 && band.GetOverviewCount() > iOverview) {
                band = band.GetOverview(iOverview);
            }

            ColorTable ct = band.GetRasterColorTable();
            if (ct == null) {
                m_ctrlLogger.Error("   Band has no color table!");
                return;
            }

            if (ct.GetPaletteInterpretation() != PaletteInterp.GPI_RGB) {
                m_ctrlLogger.Error("   Only RGB palette interp is supported by this sample!");
                return;
            }

            //================================================================
            // Get the width and height of the Dataset
            //================================================================
            int width = band.XSize;
            int height = band.YSize;

            //================================================================
            // Create a Bitmap to store the GDAL image in
            //================================================================
            m_tBitmap = new Bitmap(width, height, PixelFormat.Format32bppRgb);

            DateTime start = DateTime.Now;

            byte[] r = new byte[width * height];

            band.ReadRaster(0, 0, width, height, r, width, height, 0, 0);
            TimeSpan renderTime = DateTime.Now - start;
            m_ctrlLogger.DebugFormat("getBitmapBuffered fetch time: {0} ms",
                renderTime.TotalMilliseconds);

            //================================================================
            // Create bitmap(SetPixel)
            //================================================================
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    ColorEntry entry = ct.GetColorEntry(r[i + j * width]);
                    Color newColor = Color.FromArgb(Convert.ToInt32(entry.c1),
                        Convert.ToInt32(entry.c2), Convert.ToInt32(entry.c3));
                    m_tBitmap.SetPixel(i, j, newColor);
                }
            }
        }

        private void getBitmapGrayBuffered(int iOverview) {
            //================================================================
            // Get the GDAL Band objects from the Dataset
            //================================================================
            Band band = m_tGdalDs.GetRasterBand(1);
            if (iOverview >= 0 && band.GetOverviewCount() > iOverview) {
                band = band.GetOverview(iOverview);
            }

            //================================================================
            // Get the width and height of the Dataset
            //================================================================
            int width = band.XSize;
            int height = band.YSize;

            //================================================================
            // Create a Bitmap to store the GDAL image in
            //================================================================
            m_tBitmap = new Bitmap(width, height, PixelFormat.Format32bppRgb);

            DateTime start = DateTime.Now;

            byte[] r = new byte[width * height];

            band.ReadRaster(0, 0, width, height, r, width, height, 0, 0);
            TimeSpan renderTime = DateTime.Now - start;
            m_ctrlLogger.DebugFormat("getBitmapBuffered fetch time: {0} ms",
                renderTime.TotalMilliseconds);

            //================================================================
            // Create bitmap(SetPixel)
            //================================================================
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    Color newColor = Color.FromArgb(Convert.ToInt32(r[i + j * width]),
                        Convert.ToInt32(r[i + j * width]), Convert.ToInt32(r[i + j * width]));
                    m_tBitmap.SetPixel(i, j, newColor);
                }
            }
        }
    }
    #endregion
}

using GdalApi.Model.Domain.Object;
using log4net;
using OSGeo.GDAL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;

namespace GdalApi.Model.Domain.Mapper {
    public class GeoTiffMapper {
        #region --- [ Logger ] ---
        private static ILog m_ctrlLogger = LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        #region --- [ Fields ] ---
        private GeoTiffObject m_tGeoTiffMerge;
        private static GeoTiffMapper m_ctrlInstance;
        private List<GeoTiffObject> m_arryGeoTiff = new List<GeoTiffObject>();
        #endregion
        #region --- [ Constructor ] ---
        private GeoTiffMapper() {
        }
        #endregion
        #region --- [ Propertys ] ---
        public Image Image {
            get {
                if (m_tGeoTiffMerge == null) {
                    return null;
                }
                return m_tGeoTiffMerge.Image;
            }
        }
        public string GdalInfoText {
            get {
                if (m_tGeoTiffMerge == null) {
                    return string.Empty;
                }
                return m_tGeoTiffMerge.gdalInfoText;
            }
        }
        #endregion
        #region --- [ Public API ] ---
        public static GeoTiffMapper getObj() {
            if (m_ctrlInstance == null) {
                m_ctrlInstance = new GeoTiffMapper();
            }
            return m_ctrlInstance;
        }
        public void addGeoTiffObject(GeoTiffObject obj) {
            m_arryGeoTiff.Add(obj);
        }
        public GeoTiffObject getGeoTiffObject(int iBandNum) {
            foreach (GeoTiffObject obj in m_arryGeoTiff) {
                if (obj.BandNumber == iBandNum) {
                    return obj;
                }
            }
            return null;
        }
        public void gdalMerge() {
            // Natural color(4,3,2)
            try {
                string[] options = new string[] { };
                Dataset image = getGeoTiffObject(5).gdalDataset;

                Band redBand = GetBand(getGeoTiffObject(4).gdalDataset, ColorInterp.GCI_RedBand);
                Band greenBand = GetBand(getGeoTiffObject(3).gdalDataset, ColorInterp.GCI_GreenBand);
                Band blueBand = GetBand(getGeoTiffObject(2).gdalDataset, ColorInterp.GCI_BlueBand);
                Band alphaBand = GetBand(image, ColorInterp.GCI_AlphaBand);

                int width = redBand.XSize;
                int height = redBand.YSize;

                Dataset outImage = Gdal.GetDriverByName("GTiff").Create(
                    "out.tif", width, height, 4, DataType.GDT_Byte, null);
                // copy the projection and geographic informations of image
                double[] geoTransformerData = new double[6];
                image.GetGeoTransform(geoTransformerData);
                outImage.SetGeoTransform(geoTransformerData);
                outImage.SetProjection(image.GetProjection());


                Band outRedBand = outImage.GetRasterBand(1);
                Band outGreenBand = outImage.GetRasterBand(2);
                Band outBlueBand = outImage.GetRasterBand(3);
                Band outAlphaBand = outImage.GetRasterBand(4);

                for (int h = 0; h < height; h++) {
                    int[] red = new int[width];
                    int[] green = new int[width];
                    int[] blue = new int[width];
                    int[] alpha = new int[width];
                    // copy each matrix row of image to the above arrays
                    redBand.ReadRaster(0, h, width, 1, red, width, 1, 0, 0);
                    greenBand.ReadRaster(0, h, width, 1, green, width, 1, 0, 0);
                    blueBand.ReadRaster(0, h, width, 1, blue, width, 1, 0, 0);
                    alphaBand.ReadRaster(0, h, width, 1, alpha, width, 1, 0, 0);

                    for (int w = 0; w < width; w++) {
                        red[w] = red[w] + 1; // any process with each pixel
                        green[w] = green[w] + 1;
                        blue[w] = blue[w] + 1;
                        alpha[w] = alpha[w] + 1;
                    }
                    // write image
                    outRedBand.WriteRaster(0, h, width, 1, red, width, 1, 0, 0);
                    outGreenBand.WriteRaster(0, h, width, 1, green, width, 1, 0, 0);
                    outBlueBand.WriteRaster(0, h, width, 1, blue, width, 1, 0, 0);
                    outAlphaBand.WriteRaster(0, h, width, 1, alpha, width, 1, 0, 0);
                }
                outImage.FlushCache();

                Gdal.wrapper_GDALTranslate("TrueColor.tif", outImage,
                    new GDALTranslateOptions(new string[] { "-ot","BYTE",
                        "-of", "GTiff", "-b","1","-b","2","-b","3","-a_nodata","255","-scale" }), null, null);

                m_tGeoTiffMerge = new GeoTiffObject("TrueColor.tif", -1);
            } catch (Exception ex) {
                m_ctrlLogger.Error(ex);
            } finally {
                GC.Collect();
            }
        }

        private Band GetBand(Dataset ImageDataSet, ColorInterp colorInterp) {
            if (colorInterp.Equals(ImageDataSet.GetRasterBand(1).GetRasterColorInterpretation())) {
                return ImageDataSet.GetRasterBand(1);
            } else if (colorInterp.Equals(ImageDataSet.GetRasterBand(2).GetRasterColorInterpretation())) {
                return ImageDataSet.GetRasterBand(2);
            } else if (colorInterp.Equals(ImageDataSet.GetRasterBand(3).GetRasterColorInterpretation())) {
                return ImageDataSet.GetRasterBand(3);
            } else {
                return ImageDataSet.GetRasterBand(4);
            }
        }
        #endregion
    }
}

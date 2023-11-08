using ImageEcoLab.Models;
using ImageEcoLab.Services;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Windows.Markup;

namespace ImageEcoLab.ViewModels
{
	[MarkupExtensionReturnType(typeof(HistogramViewModel))]
	internal class HistogramViewModel : ViewModel
	{
		private readonly ImageEngine _imageEngine;

		#region Histograms
		private Histograms _histograms;
		#endregion

		#region BrightnessHist
		private PlotModel _brightnessHist = new PlotModel();
		public PlotModel BrightnessHist { get => _brightnessHist; set => Set(ref _brightnessHist, value); }
		#endregion

		#region Property RedChannelHist
		private PlotModel _redChannelHist = new PlotModel();
		public PlotModel RedChannelHist { get => _redChannelHist; set => Set(ref _redChannelHist, value); }
		#endregion

		#region Property GreenChannelHist
		private PlotModel _greenChannelHist = new PlotModel();
		public PlotModel GreenChannelHist { get => _greenChannelHist; set => Set(ref _greenChannelHist, value); }
		#endregion

		#region Property BlueChannelHist
		private PlotModel _blueChannelHist = new PlotModel();
		public PlotModel BlueChannelHist { get => _blueChannelHist; set => Set(ref _blueChannelHist, value); }
		#endregion

		public void UpdateHistograms(ImageModel currentImageModel, short alignCoef, bool isLinearGraph)
		{
			this._histograms = _imageEngine.GetHistograms(currentImageModel.Bytes, currentImageModel.BitsPerPixel);

			var redChannel = this._histograms.RedChannel;
			var greenChannel = this._histograms.GreenChannel;
			var blueChannel = this._histograms.BlueChannel;
			var brightness = this._histograms.BrightnessHist;

			RedChannelHist = CreatePlotModel(redChannel, OxyColor.FromRgb(255, 0, 0), isLinearGraph, alignCoef);
			GreenChannelHist = CreatePlotModel(greenChannel, OxyColor.FromRgb(0, 255, 0), isLinearGraph, alignCoef);
			BlueChannelHist = CreatePlotModel(blueChannel, OxyColor.FromRgb(0, 0, 255), isLinearGraph, alignCoef);
			BrightnessHist = CreatePlotModel(brightness, OxyColor.FromRgb(0, 0, 0), isLinearGraph, alignCoef);
		}

		private LineSeries CreateLine(long[] data, OxyColor color)
		{
			var line = new LineSeries();

			line.Color = color;

			for (int i = 0; i < data.Length; ++i)
			{
				line.Points.Add(new DataPoint(i, data[i]));
			}

			return line;
		}

		private HistogramSeries CreateHistogram(long[] data, OxyColor color)
		{
			var series = new HistogramSeries()
			{
				FillColor = color
			};

			for (int i = 0; i < data.Length; ++i)
			{
				var item = new HistogramItem(i, i + 1, data[i], 10, color);
				series.Items.Add(item);
			}

			return series;
		}

		private PlotModel CreatePlotModel(long[] data, OxyColor color, bool isLinearGraph = true, short aligmentCoefHist = 1)
		{
			var plotModel = new PlotModel();

			XYAxisSeries series;
			var alignedData = _imageEngine.AlignChannelHeight(data, aligmentCoefHist);
			if (isLinearGraph)
			{
				series = CreateLine(alignedData, color);
			}
			else
			{
				series = CreateHistogram(alignedData, color);
			}

			plotModel.Series.Add(series);

			return plotModel;
		}

		public HistogramViewModel()
		{
			if (!App.IsDesignMode)
			{
				throw new InvalidOperationException("Cannot use this constructor in release");
			}
		}

        public HistogramViewModel(ImageEngine imageEngine)
        {
			_imageEngine = imageEngine;
        }
    }
}

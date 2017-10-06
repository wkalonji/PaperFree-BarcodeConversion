using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace BarcodeConversion.App_Code
{

	// Not utilized
	public enum BarcodeWeight
	{
		Small = 1,
		Medium = 2,
		Large = 3
	}

	public class Code39Barcode
	{
		#region Private Member Variables
		private const int SpacingBetweenBarcodeAndText = 5;
		private const string InterchangeGap = "0";
		private readonly string code39alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-. $/+%*";
		private readonly string[] code39code =
		{
		/* 0 */ "000110100", 
		/* 1 */ "100100001", 
		/* 2 */ "001100001", 
		/* 3 */ "101100000",
		/* 4 */ "000110001", 
		/* 5 */ "100110000", 
		/* 6 */ "001110000", 
		/* 7 */ "000100101",
		/* 8 */ "100100100", 
		/* 9 */ "001100100", 
		/* A */ "100001001", 
		/* B */ "001001001",
		/* C */ "101001000", 
		/* D */ "000011001", 
		/* E */ "100011000", 
		/* F */ "001011000",
		/* G */ "000001101", 
		/* H */ "100001100", 
		/* I */ "001001100", 
		/* J */ "000011100",
		/* K */ "100000011", 
		/* L */ "001000011", 
		/* M */ "101000010", 
		/* N */ "000010011",
		/* O */ "100010010", 
		/* P */ "001010010", 
		/* Q */ "000000111", 
		/* R */ "100000110",
		/* S */ "001000110", 
		/* T */ "000010110", 
		/* U */ "110000001", 
		/* V */ "011000001",
		/* W */ "111000000", 
		/* X */ "010010001", 
		/* Y */ "110010000", 
		/* Z */ "011010000",
		/* - */ "010000101", 
		/* . */ "110000100", 
		/*' '*/ "011000100",
		/* $ */ "010101000",
		/* / */ "010100010", 
		/* + */ "010001010", 
		/* % */ "000101010", 
		/* * */ "010010100"
	};
		#endregion

		#region Constructors
		public Code39Barcode() : this(string.Empty) { }
		public Code39Barcode(string barcodeText) : this(barcodeText, 75) { }
		public Code39Barcode(string barcodeText, int height)
		{
			this.BarcodeText = barcodeText;
			this.Height = height;
			this.BarcodePadding = 0;
			this.ShowBarcodeText = false;
			this.BarcodeWeight = BarcodeWeight.Small;
			//this.BarcodeTextFont = new Font("Calibri", 9.0F);
            this.BarcodeTextFont = new Font("Arial", 9.0F);
            this.ImageFormat = ImageFormat.Gif;
		}
		#endregion

		#region Properties
		public string BarcodeText { get; set; }
		public BarcodeWeight BarcodeWeight { get; set; }
		public int BarcodePadding { get; set; }
		public int Height { get; set; }
		public bool ShowBarcodeText { get; set; }
		public Font BarcodeTextFont { get; set; }
		public ImageFormat ImageFormat { get; set; }
		#endregion

		public byte[] Generate()
		{
			// Ensure Barcode property has been set
			if (string.IsNullOrWhiteSpace(this.BarcodeText))
				throw new ArgumentException("Barcode must be set prior to calling Generate");

			// Ensure Barcode does not contain invalid characters
			for (var i = 0; i < this.BarcodeText.Length; i++)
				if (code39alphabet.IndexOf(this.BarcodeText[i]) == -1)
					throw new ArgumentException(string.Format("Invalid character for Barcode: '{0}' is not a valid code 39 character", this.BarcodeText[i]));

			// Create the encoded string
			var codeToGenerate = "*" + this.BarcodeText + "*";
			var encodedString = string.Empty;
			for (var i = 0; i < codeToGenerate.Length; i++)
			{
				if (i > 0)
					encodedString += InterchangeGap;
				encodedString += code39code[code39alphabet.IndexOf(codeToGenerate[i])];
			}

			// Determine the barcode width
			var widthOfBarcodeString = 0;
			var wideToNarrowRatio = 3.0;
			for (var i = 0; i < encodedString.Length; i++)
			{
				if (encodedString[i] == '1')
					widthOfBarcodeString += (int)(wideToNarrowRatio * (int)this.BarcodeWeight);
				else
					widthOfBarcodeString += (int)this.BarcodeWeight;
			}
			var widthOfImage = widthOfBarcodeString + (BarcodePadding * 2);

			// Create Bitmap class
			using (var bmp = new Bitmap(widthOfImage, this.Height, PixelFormat.Format32bppArgb))
			{
				using (var gfx = Graphics.FromImage(bmp))
				{
					// Start with a white background
					gfx.FillRectangle(Brushes.White, 0, 0, bmp.Width, bmp.Height);

					// Determine offset to center BarcodeText and the height of Barcode
					var barcodeTextSize = gfx.MeasureString(this.BarcodeText, this.BarcodeTextFont);
					var barcodeTextX = (widthOfImage - (int)barcodeTextSize.Width) / 2;
					var barcodeHeight = this.Height - (this.BarcodePadding * 2);
					if (this.ShowBarcodeText)
						barcodeHeight -= SpacingBetweenBarcodeAndText + (int)barcodeTextSize.Height;

					var x = this.BarcodePadding;
					var barcodeTop = this.BarcodePadding;
					var barcodeBottom = barcodeTop + barcodeHeight;

					for (var i = 0; i < encodedString.Length; i++)
					{
						var lineWidth = 0;

						if (encodedString[i] == '1')
							lineWidth = (int)(wideToNarrowRatio * (int)this.BarcodeWeight);
						else
							lineWidth = (int)this.BarcodeWeight;

						gfx.FillRectangle(i % 2 == 0 ? Brushes.Black : Brushes.White, x, barcodeTop, lineWidth, barcodeBottom);

						x += lineWidth;
					}

					if (this.ShowBarcodeText)
					{
						var barcodeTextTop = barcodeBottom + SpacingBetweenBarcodeAndText;
                        gfx.DrawString(this.BarcodeText, this.BarcodeTextFont, Brushes.Black, barcodeTextX, barcodeTextTop);
                    }

					var output = new MemoryStream();
					bmp.Save(output, this.ImageFormat);
					return output.ToArray();
				}
			}
		}
	}
}
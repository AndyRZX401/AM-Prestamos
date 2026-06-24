using System;
using System.Drawing;
using System.IO;

class Program {
    static void Main(string[] args) {
        if (args.Length < 2) return;
        string pngPath = args[0];
        string icoPath = args[1];
        try {
            using (Bitmap bmp = new Bitmap(pngPath)) {
                using (FileStream fs = new FileStream(icoPath, FileMode.Create)) {
                    fs.WriteByte(0); fs.WriteByte(0);
                    fs.WriteByte(1); fs.WriteByte(0);
                    fs.WriteByte(1); fs.WriteByte(0);
                    
                    int width = bmp.Width;
                    int height = bmp.Height;
                    fs.WriteByte((byte)(width > 255 ? 0 : width));
                    fs.WriteByte((byte)(height > 255 ? 0 : height));
                    fs.WriteByte(0); fs.WriteByte(0);
                    fs.WriteByte(1); fs.WriteByte(0);
                    fs.WriteByte(32); fs.WriteByte(0);
                    
                    using (MemoryStream ms = new MemoryStream()) {
                        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        byte[] pngBytes = ms.ToArray();
                        byte[] sizeBytes = BitConverter.GetBytes(pngBytes.Length);
                        fs.Write(sizeBytes, 0, 4);
                        
                        byte[] offsetBytes = BitConverter.GetBytes(22);
                        fs.Write(offsetBytes, 0, 4);
                        
                        fs.Write(pngBytes, 0, pngBytes.Length);
                    }
                }
            }
            Console.WriteLine("Conversión exitosa.");
        } catch (Exception ex) {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}

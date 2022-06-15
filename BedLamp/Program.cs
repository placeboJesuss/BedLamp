using System;

using System.Net.Sockets;
using System.Text;
using System.Timers;
using NAudio.CoreAudioApi;

namespace BedLamp
{
    class Led
    {
        public int r, g, b;
        
        public Led()
        {
            r = 0;
            g = 0;
            b = 0;
        }
        public Led(int R, int G, int B)
        {
            r = R;
            g = G;
            b = B;
        }
      
        public Led setRGB(int R, int G, int B)
        {
            r = R;
            g = G;
            b = B;
            return this;
        }

        override public string ToString()
        {
            return string.Format("{0:X2}{1:X2}{2:X2}", (int)(r), (int)(g), (int)(b));
        }
    }


    class Program
    {
        static int n = 60;
        static void Main(string[] args)
        {
            UdpClient  client = new UdpClient("192.168.0.169", 4210);
            Console.WriteLine("Hello World!");
            MMDeviceEnumerator devEnum = new MMDeviceEnumerator();
            MMDevice defaultDevice = devEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            float[] chek = new float[500];
            int pos = 0;
            float sum = 0, max = 0;
            while (true)
            {
                float level = defaultDevice.AudioMeterInformation.MasterPeakValue;
                if(level>0.001)
                    add(chek, level, ref pos, ref sum, ref max);
                byte[] data = musicGradient(level, sum/chek.Length , max);
                if(data!= null)
                    client.Send(data, data.Length);
                System.Threading.Thread.Sleep(10);
            }

        }

        private static void add(float[] chek, float k, ref int pos, ref float sum, ref float max)
        {
            if (pos == chek.Length)
                pos = 0;
            bool delMax = chek[pos] == max;
            sum -= chek[pos];
            sum += k;
            chek[pos] = k;
            if (delMax)
            {
                max = chek[0];
                for (int i = 1; i < chek.Length; i++)
                    if (chek[i] > max)
                        max = chek[i];
            }
        }


      
        static int br;
        static float counter;
        static int s;
        static DateTime dt;

        /// <summary>
        /// Form data for UDP request
        /// </summary>
        private static byte[] musicGradient(float brr, float sum, float max)
        {
            Led led = new Led();
            int bright = (int)(brr * 255);
            string request = "2";
            int rr = bright;
            int r, g, b;

            rr = (int)(0.8 * rr / max + 0.2 * rr/sum * 0.5); //1
            if (rr > 255) rr = 255;

            if (rr > br) br = (int)(br * 0.3 + rr * 0.7);
            else br = (int)(br * 0.8 + rr * 0.2);
            int i;
            if (s == 0 && (int)(n / 2 * br / 255f + 0.5) == 0) return null;
            s = (int)(n / 2 * Math.Pow(br / 255f,1.4f)  + 0.5);

            Console.WriteLine(rr.ToString() + " " + s.ToString());
            if (s > n / 2) s = n / 2;
            if (s < 0) s = 0;

            for (i = 0; i < s; i++)
            {
                double hue = counter - i * 8;
                double value = ((0.2 + 0.8 * (s - i) / s) * (s * 2f / n));
                HsvToRgb(hue,1,value,out r, out g, out b);
                request += led.setRGB(r,g,b).ToString();
            }
            for (; i < n / 2; i++)
            {
                request += led.setRGB(0, 0, 0).ToString();
            }
            
            counter +=  (float)(DateTime.Now - dt).TotalMilliseconds / 7f * s/30f;
            dt = DateTime.Now;
            if (counter >= 360) counter = 0;
            
            return Encoding.UTF8.GetBytes(request);
        }

        /// <summary>
        /// Convert HSV to RGB
        /// </summary>
        static void HsvToRgb(double h, double S, double V, out int r, out int g, out int b)
        {
            double H = h;
            while (H < 0) { H += 360; };
            while (H >= 360) { H -= 360; };
            double R, G, B;
            if (V <= 0)
            { R = G = B = 0; }
            else if (S <= 0)
            {
                R = G = B = V;
            }
            else
            {
                double hf = H / 60.0;
                int i = (int)Math.Floor(hf);
                double f = hf - i;
                double pv = V * (1 - S);
                double qv = V * (1 - S * f);
                double tv = V * (1 - S * (1 - f));
                switch (i)
                {

                    // Red is the dominant color

                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;

                    // Green is the dominant color

                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;

                    // Blue is the dominant color

                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;

                    // Red is the dominant color

                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // The color is not defined, we should throw an error.

                    default:
                        //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                        R = G = B = V; // Just pretend its black/white
                        break;
                }
            }
            r = Clamp((int)(R * 255.0));
            g = Clamp((int)(G * 255.0));
            b = Clamp((int)(B * 255.0));
        }

        /// <summary>
        /// Clamp a value to 0-255
        /// </summary>
        static int Clamp(int i)
        {
            if (i < 0) return 0;
            if (i > 255) return 255;
            return i;
        }
    }    

}

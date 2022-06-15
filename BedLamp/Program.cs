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
         int time;
        float br;

        public Led()
        {
            r = 100;
            g = 100;
            b = 100;

            br = 1;
            time = 0;
        }
        public float Bright
        {
            get
            {
                return br;
            }
            set
            {
                br = value;
            }
        }
        public void setRGB(int R, int G, int B)
        {
            r = R;
            g = G;
            b = B;
        }
        public void setRGB(int R, int G, int B, float bright, int Time)
        {
            r = R;
            g = G;
            b = B;
            br = bright;
            time = Time;
        }

        override public string ToString()
        {
            return string.Format("{0:X2}{1:X2}{2:X2}", (int)(br * r), (int)(br * g), (int)(br * b));
        }
        public void Tick(int deltaTime)
        {
            Random rand = new Random();
            br -= 1/30f;
            int ll = 2;
            r = (r + rand.Next(0, ll))%255;
            g  =(g + rand.Next(0, ll)) % 255;
            b = (b + rand.Next(0, ll)) % 255;
            if (br <= 0.05)
            {
                br = 0;
             //  r = (byte)(rand.Next(0, 255));
              //  g = (byte)(rand.Next(0, 255));
             //   b = (byte)(rand.Next(0, 255));
            }
        }
    }


    class Program
    {
        static int n = 60;
        static byte[] data;
        static Led[] leds;
        static UdpClient client;
        static void Main(string[] args)
        {
            client = new UdpClient("192.168.0.169", 4210);
            Console.WriteLine("Hello World!");
            MMDeviceEnumerator devEnum = new MMDeviceEnumerator();
            MMDevice defaultDevice = devEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            leds = new Led[n];
            for (int i = 0; i < n; i++)
                leds[i] = new Led();
            float[] chek = new float[500];
            int pos = 0;
            float sum = 0, max = 0;
            while (true)
            {
                float level = defaultDevice.AudioMeterInformation.MasterPeakValue;
                if(level>0.001)
                    add(chek, level, ref pos, ref sum, ref max);
                musicGradient(level, sum/chek.Length , max);
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


        static int cc = 0;
      
        static int br;
        static float counter;
        static int s;
        static DateTime dt;
       



        private static void musicGradient(float brr, float sum, float max)
        {
            int bright = (int)(brr * 255);
            string request = "2";
            int rr = bright;
            int r, g, b;

            rr = (int)(0.8 * rr / max + 0.2 * rr/sum * 0.5); //1
            if (rr > 255) rr = 255;

            if (rr > br) br = (int)(br * 0.3 + rr * 0.7);
            else br = (int)(br * 0.8 + rr * 0.2);
            int i;
            if (s == 0 && (int)(n / 2 * br / 255f + 0.5) == 0) return;
            s = (int)(n / 2 * Math.Pow(br / 255f,1.4f)  + 0.5);

            Console.WriteLine(rr.ToString() + " " + s.ToString());
            if (s > n / 2) s = n / 2;
            if (s < 0) s = 0;

            for (i = 0; i < s; i++)
            {
                HsvToRgb(counter - i * 8, 1, ((0.2 + 0.8 * (s - i) / s) * (s*2f/n)), out r, out g, out b);
                leds[i].setRGB(r,g,b);
                request += leds[i].ToString();
            }
            for (; i < n / 2; i++)
            {
                leds[i].setRGB(0, 0, 0);
                request += leds[i].ToString();
            }
            data = Encoding.UTF8.GetBytes(request);
            cc++;
            client.Send(data, data.Length);

            counter +=  (float)(DateTime.Now - dt).TotalMilliseconds / 7f * s/30f;
            dt = DateTime.Now;
            if (counter >= 360) counter = 0;

        }
        static void round()
        {
            for (int i = 0; i < n / 2-1; i++)
            {
                leds[i].Bright = leds[i + 1].Bright;
                
                leds[n-1 - i].Bright = leds[n - i - 2].Bright;

            }
            string request = "1";
            for (int i = 0; i < n; i++)
                request += leds[i].ToString();
            data = Encoding.UTF8.GetBytes(request);
            client.Send(data, data.Length);
        }
        static void lines(int bright)
        {
            if (bright / 255f > 0.05)
            {
                leds[n / 2].Bright = (float)Math.Pow(bright / 255f, 3);
                leds[n / 2 - 1].Bright = (float)Math.Pow(bright / 255f, 3);
            }
            else
            {
                leds[n / 2].Bright = 0f;
                leds[n / 2 - 1].Bright = 0f;
            }
        }

        //private static void plugin()
        //{
        //    while (port.BytesToRead > 0)
        //    {
        //        while (port.ReadByte() == 65)
        //            if (port.ReadByte() == 100)
        //                if (port.ReadByte() == 97)
        //                    if (port.ReadByte() == 0)
        //                    {
        //                        string request = "2";
        //                        int num;
        //                        num = port.ReadByte() + 1;
        //                        port.ReadByte();
        //                        int br = 0;
        //                        for (int j = 0; j < num; j++)
        //                        {
        //                            br += port.ReadByte();

        //                            port.ReadByte();
        //                            port.ReadByte();
        //                        }
        //                        br = br / num;
        //                        int s = (int)(n / 2 * br / 255f + 0.5);
        //                        int i,r,g,b;
        //                        for (i = 0; i < s; i++)
        //                        {
        //                            HsvToRgb(counter - i * 10, 1, ((0.2 + 0.8 * (s - i) / s) *br / 255f), out r, out g, out b);
        //                            request += string.Format("{0:X2}{1:X2}{2:X2}", (int)(r), (int)(g), (int)(b));

        //                        }
        //                        // request += string.Format("{0:X2}{1:X2}{2:X2}", (int)(255), (int)(0), (int)(0));
        //                        for (; i < n / 2; i++)
        //                        {
        //                            request += string.Format("{0:X2}{1:X2}{2:X2}", (int)(0), (int)(0), (int)(0));

        //                        }

        //                        data = Encoding.UTF8.GetBytes(request);
        //                        cc++;
        //                        client.Send(data, data.Length);
        //                        counter += 3.5f * br / 255f;
        //                       // counter += 7f * br / 255f;
        //                        if (counter >= 360) counter = 0;
        //                    }
        //    }
        //}
        //private static void direct()
        //{
        //    while (port.BytesToRead > 0)
        //    {
        //        while (port.ReadByte() == 65)
        //            if (port.ReadByte() == 100)
        //                if (port.ReadByte() == 97)
        //                    if (port.ReadByte() == 0)
        //                    {
        //                        string request = "1";
        //                        int num;
        //                        num = port.ReadByte() + 1;
        //                        port.ReadByte();
        //                        for (int j = 0; j < 30; j++)
        //                        {
        //                            int r = port.ReadByte();
        //                            int g = port.ReadByte();
        //                            int b = port.ReadByte();

        //                            request += string.Format("{0:X2}{1:X2}{2:X2}", (int)(r), (int)(g), (int)(b));
        //                        }
        //                        data = Encoding.UTF8.GetBytes(request);
        //                        client.Send(data, data.Length);
        //                        n++;
        //                    }
        //    }
        //}


        //    private static void Sp_DataReceeived(object sender, SerialDataReceivedEventArgs e)
        //    {
        //        int bytes = sp.BytesToRead;
        //        for (int i = 0; i < bytes; i++)
        //        {
        //            while (sp.ReadByte() == 65)
        //                if (sp.ReadByte() == 100)
        //                    if (sp.ReadByte() == 97)
        //                        if (sp.ReadByte() == 0)
        //                        {
        //                            string request;
        //                            int num;
        //                            num = sp.ReadByte() + 1;
        //                            request = num.ToString();
        //                            sp.ReadByte();
        //                            for (int j = 0; j < num * 3; j++)
        //                            {
        //                                int r = sp.ReadByte();
        //                                int g = sp.ReadByte();
        //                                int b = sp.ReadByte();

        //                                request += "&" + string.Format("{0:X2}{1:X2}{2:X2}", (int)(r), (int)(g), (int)(b));
        //                            }
        //                            data = Encoding.UTF8.GetBytes(request);
        //                            client.Send(data, data.Length);
        //                            n++;
        //                        }
        //        }
        //    }

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

        //    private static void Sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        //    {
        //        int bytes = sp.BytesToRead;
        //        for (int i = 0; i < bytes; i++)
        //        {
        //            while (sp.ReadByte() == 65)
        //                if (sp.ReadByte() == 100)
        //                    if (sp.ReadByte() == 97)
        //                        if (sp.ReadByte() == 0)
        //                        {
        //                            string request;
        //                            int num;
        //                            num = 30;
        //                            request = num.ToString();
        //                            sp.ReadByte();
        //                            sp.ReadByte();
        //                            char r = (char)sp.ReadByte();

        //                            sp.ReadByte();

        //                            sp.ReadByte();
        //                            sp.ReadByte();
        //                            char g = (char)sp.ReadByte();
        //                            sp.ReadByte();
        //                            sp.ReadByte();
        //                            sp.ReadByte();
        //                            char b = (char)sp.ReadByte();

        //                            //  br = (r + g + b) / 765f * 0.65f + br * 0.35f ;
        //                            if ((r + g + b) / 765f >= br)
        //                            {
        //                                br = (r + g + b) / 765f;
        //                            }
        //                            else
        //                            {
        //                                br = (r + g + b) / 765f * 0.6f + br * 0.4f; ;
        //                                // if (br < 0) br = 0;
        //                            }
        //                            int fn = (int)(num * br);
        //                            string[] mas = new string[fn / 2];
        //                            for (int j = 0; j < fn / 2; j++)
        //                                mas[j] = "&" + string.Format("{0:X2}{1:X2}{2:X2}", (int)(r * br), (int)(g * br), (int)(b * br));
        //                            for (int j = 0; j < (num / 2 - fn / 2); j++)
        //                                request += "&" + string.Format("{0:X2}{1:X2}{2:X2}", 0, 0, 0);
        //                            for (int j = fn / 2 - 1; j >= 0; j--)
        //                            {
        //                                request += mas[j];
        //                            }
        //                            for (int j = 0; j < fn / 2; j++)
        //                            {
        //                                request += mas[j];
        //                            }
        //                            for (int j = 0; j < (num / 2 - fn / 2); j++)
        //                                request += "&" + string.Format("{0:X2}{1:X2}{2:X2}", 0, 0, 0);
        //                            counter++;
        //                            if (counter >= 360) counter = 0;


        //                            Console.WriteLine(request.Length);
        //                            //Console.WriteLine(br);
        //                            //Console.WriteLine("ok");
        //                            data = Encoding.UTF8.GetBytes(request);
        //                            client.Send(data, data.Length);
        //                            n++;
        //                        }
        //        }
        //    }
        //}
    }    

}

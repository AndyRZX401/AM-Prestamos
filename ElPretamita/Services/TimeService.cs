using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ElPretamita.Services
{
    public static class TimeService
    {
        private static TimeSpan _offset = TimeSpan.Zero;
        private static bool _initialized = false;
        private static readonly string _offsetFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "timeoffset.dat");

        // Obtiene la hora real anti-fraudes
        public static DateTime GetRealTime()
        {
            if (!_initialized)
            {
                // Si alguien pide la hora antes de que termine de inicializarse de forma asíncrona,
                // intentamos cargar el offset guardado como plan B
                CargarOffsetLocal();
            }
            return DateTime.Now.Add(_offset);
        }

        public static async Task InitializeAsync()
        {
            try
            {
                // Intentar obtener la hora de internet
                DateTime networkTime = await GetNetworkTimeAsync();
                
                // Calcular el desfase con la hora local de la PC
                _offset = networkTime - DateTime.Now;
                
                // Guardar este desfase para cuando no haya internet
                GuardarOffsetLocal(_offset);
                _initialized = true;
                
                System.Diagnostics.Debug.WriteLine($"[TimeService] Sincronizado. Offset: {_offset.TotalSeconds} seg.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TimeService] Error NTP, usando fallback. Detalle: {ex.Message}");
                // Si no hay internet, cargamos el último desfase guardado
                CargarOffsetLocal();
                _initialized = true;
            }
        }

        private static Task<DateTime> GetNetworkTimeAsync()
        {
            return Task.Run(() =>
            {
                const string ntpServer = "pool.ntp.org";
                var ntpData = new byte[48];
                ntpData[0] = 0x1B; // NTP version 3, client mode

                var addresses = Dns.GetHostEntry(ntpServer).AddressList;
                var ipEndPoint = new IPEndPoint(addresses[0], 123);

                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    socket.ReceiveTimeout = 3000;
                    socket.Connect(ipEndPoint);
                    socket.Send(ntpData);
                    socket.Receive(ntpData);
                }

                // Parse NTP Data
                const byte serverReplyTime = 40;
                ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);
                ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

                // Convert to little-endian
                intPart = SwapEndianness(intPart);
                fractPart = SwapEndianness(fractPart);

                var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
                
                var networkDateTime = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds((long)milliseconds);

                // Convertir a hora local para que coincida con la zona horaria real
                return networkDateTime.ToLocalTime();
            });
        }

        private static uint SwapEndianness(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
                          ((x & 0x0000ff00) << 8) +
                          ((x & 0x00ff0000) >> 8) +
                          ((x & 0xff000000) >> 24));
        }

        private static void GuardarOffsetLocal(TimeSpan offset)
        {
            try
            {
                File.WriteAllText(_offsetFilePath, offset.Ticks.ToString());
            }
            catch { }
        }

        private static void CargarOffsetLocal()
        {
            try
            {
                if (File.Exists(_offsetFilePath))
                {
                    string content = File.ReadAllText(_offsetFilePath);
                    if (long.TryParse(content, out long ticks))
                    {
                        _offset = new TimeSpan(ticks);
                    }
                }
            }
            catch { }
        }
    }
}

namespace SonoGuard.Services
{
    public class MicrophoneService : IMicrophoneService
    {
        public bool EstaActivo { get; private set; }
        public event Action<double>? NuevaMedicion;

        private CancellationTokenSource? _cts;
        private bool _disposed;

        public async Task<bool> SolicitarPermisoAsync()
        {
            var estado = await Permissions.CheckStatusAsync<Permissions.Microphone>();
            if (estado != PermissionStatus.Granted)
                estado = await Permissions.RequestAsync<Permissions.Microphone>();
            return estado == PermissionStatus.Granted;
        }

        public void IniciarMedicion()
        {
            if (EstaActivo) return;
            EstaActivo = true;
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

#if ANDROID
            Task.Run(() => MedirAndroid(token), token);
#else
            Task.Run(async () => await MedirSimulado(token), token);
#endif
        }

#if ANDROID
        private void MedirAndroid(CancellationToken token)
        {
            int sampleRate    = 44100;
            int bufferSize    = Android.Media.AudioRecord.GetMinBufferSize(
                                    sampleRate,
                                    Android.Media.ChannelIn.Mono,
                                    Android.Media.Encoding.Pcm16bit);

            if (bufferSize <= 0)
                bufferSize = 4096;

            var recorder = new Android.Media.AudioRecord(
                Android.Media.AudioSource.Mic,
                sampleRate,
                Android.Media.ChannelIn.Mono,
                Android.Media.Encoding.Pcm16bit,
                bufferSize);

            try
            {
                recorder.StartRecording();

                short[] buffer = new short[bufferSize / 2];

                while (!token.IsCancellationRequested)
                {
                    int leidos = recorder.Read(buffer, 0, buffer.Length);

                    if (leidos > 0)
                    {
                        double amplitudMax = 0;
                        for (int i = 0; i < leidos; i++)
                        {
                            double abs = Math.Abs(buffer[i]);
                            if (abs > amplitudMax) amplitudMax = abs;
                        }

                        double db = amplitudMax > 0
                            ? 20.0 * Math.Log10(amplitudMax / 32767.0) + 90.0
                            : 20.0;

                        db = Math.Clamp(db, 20.0, 120.0);

                        MainThread.BeginInvokeOnMainThread(
                            () => NuevaMedicion?.Invoke(db));
                    }

                    Thread.Sleep(500);
                }
            }
            finally
            {
                recorder.Stop();
                recorder.Release();
                recorder.Dispose();
            }
        }
#endif

        private async Task MedirSimulado(CancellationToken token)
        {
            var rng      = new Random();
            double base_ = 38.0;

            while (!token.IsCancellationRequested)
            {
                double delta = (rng.NextDouble() * 16) - 8;
                double pico  = rng.NextDouble() < 0.08 ? rng.NextDouble() * 45 : 0;
                double nivel = Math.Clamp(base_ + delta + pico, 20, 120);

                MainThread.BeginInvokeOnMainThread(() => NuevaMedicion?.Invoke(nivel));

                await Task.Delay(500, token).ContinueWith(_ => { });
            }
        }

        public void DetenerMedicion()
        {
            if (!EstaActivo) return;
            EstaActivo = false;
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        public void Dispose()
        {
            if (_disposed) return;
            DetenerMedicion();
            _disposed = true;
        }
    }
}

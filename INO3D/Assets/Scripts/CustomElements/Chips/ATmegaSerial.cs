using CircularBuffer;
using System.Globalization;
using System.Text;
using System;

namespace Assets.Scripts.CustomElements.Chips
{
    public class ATmegaSerial
    {
        #region Properties

        public Action<byte> OnArduinoSend;

        #endregion

        #region Fields

        private readonly int bufferSize;
        private readonly CircularBuffer<byte> rxBuffer;

        #endregion

        #region Constructor

        public ATmegaSerial(int bufferSize)
        {
            this.bufferSize = bufferSize;
            rxBuffer = new CircularBuffer<byte>(this.bufferSize);
        }

        #endregion

        #region Public Methods

        public void Begin(int baud)
        {
        }

        public void End()
        {
        }

        public int Available()
        {
            return (bufferSize + rxBuffer.Size) % bufferSize;
        }

        public int Peek()
        {
            if (rxBuffer.Size == 0)
                return -1;
            return rxBuffer[rxBuffer.Size - 1];
        }

        public int Read()
        {
            if (rxBuffer.Size == 0)
                return -1;

            var c = rxBuffer[rxBuffer.Size - 1];
            rxBuffer.PopBack();
            return c;
        }

        public void Flush()
        {
        }

        public int Print(object value, int format)
        {
            var bytes = Encoding.UTF8.GetBytes(ObjectToString(value, format));
            foreach (var b in bytes)
                Write(b);
            return bytes.Length;
        }

        public int Println(object value, int format)
        {
            var bytes = Encoding.UTF8.GetBytes(ObjectToString(value, format) + "\n");
            foreach (var b in bytes)
                Write(b);
            return bytes.Length;
        }

        public void WriteToArduino(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            foreach (var b in bytes)
                rxBuffer.PushFront(b);
        }

        #endregion

        #region Private Methods

        private int Write(byte c)
        {
            OnArduinoSend?.Invoke(c);
            return 1;
        }

        private string ObjectToString(object value, int format)
        {
            if (value is string stringValue)
                return stringValue;

            if (value is short intValue)
            {
                var formattedValue = intValue.ToString();
                if (format > -1)
                {
                    try
                    {
                        formattedValue = Convert.ToString(intValue, format);
                    }
                    catch
                    {
                        formattedValue = intValue.ToString();
                    }
                }

                return formattedValue;
            }

            if (value is float floatValue)
            {
                if (format > -1)
                    floatValue = (float) Math.Round(floatValue, format);
                return floatValue.ToString(CultureInfo.InvariantCulture);
            }

            return value.ToString();
        }

        #endregion
    }
}
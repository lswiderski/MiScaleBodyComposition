using MiScaleBodyComposition.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiScaleBodyComposition
{
    public class LegacyMiScale
    {
        private double _weight;
        private double _height;
        private byte[] _data;

        private const int bufferSize = 10;

        public BodyComposition GetWeight(byte[] data, double height)
        {
            CheckInput(data);
            var isStabilized = this.Istabilized(_data);
            if (!isStabilized)
            {
                throw new NotStabilizedException(
                    "Data from mi scale are not stabilized. Wait until the end of measurement");
            }
            this._weight = this.CalculateWeight(_data);
            this._height = height;

            return this.GetBodyComposition();
        }

        public bool Istabilized(byte[] data)
        {
            this.CheckInput(data);
            var ctrlByte1 = _data[0];
            var stabilized = ctrlByte1 & (1 << 5);
            return stabilized > 0;
        }


        private BodyComposition GetBodyComposition()
        {
            return new BodyComposition
            {
                Weight = _weight,
                BMI = Math.Round(this.GetBmi(), 1),
                Day = _data[6],
                Month = _data[5],
                Hour = _data[7],
                Minute = _data[8],
                Year = ((_data[4] & 0xFF) << 8 | ((_data[3] & 0xFF)))
            };
        }

        private double GetBmi()
        {
            return this.CheckValueOverflow(this._weight / ((this._height / 100) * (this._height / 100)), 10, 90);
        }
        private double CheckValueOverflow(double value, double min, double max)
        {
            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

        private double CalculateWeight(byte[] data)
        {
            return (((data[2] & 0xFF) << 8) | (data[1] & 0xFF)) * 0.005;
        }

        private void CheckInput(byte[] data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data), "data cannot be empty");
            }

            if (data.Length < bufferSize)
            {
                throw new DataLengthException($"data must by at least {bufferSize} bytes long");
            }

            if (data.Length > bufferSize)
            {
                data = data.Skip(data.Length - bufferSize).ToArray();
            }

            _data = data;
        }
    }
}

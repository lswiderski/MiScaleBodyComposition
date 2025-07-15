using MiScaleBodyComposition.Contracts;
using MiScaleBodyComposition.Exceptions;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiScaleBodyComposition
{
    public class S400Scale
    {
        private const string Weight = "weight";
        private const string HeartRate = "heartRate";
        private const string Impedance = "impedance";
        private const string ImpedanceLow = "impedanceLow";

        private const int bufferSize = 24;
        private byte[] _data;

        private double _weight;
        private double _impedance;
        private double _height;
        private double _age;
        private Sex _sex;
        private Dictionary<string, double> sensors;

        private void UpdateSensor(string key, double value)
        {
            if (sensors == null)
            {
                sensors = new Dictionary<string, double>();
            }
            if (sensors.ContainsKey(key))
            {
                sensors[key] = value;
            }
            else
            {
                sensors.Add(key, value);
            }
        }

        private double? GetSensorValue(string key)
        {
            if (sensors != null && sensors.ContainsKey(key))
            {
                return sensors[key];
            }
            return null;
        }

        public static byte[] StringToByteArray(string hex)
        {
            if (string.IsNullOrEmpty(hex) || hex.Length % 2 != 0)
            {
                return Array.Empty<byte>();
            }

            // Remove any potential whitespace
            hex = hex.Trim();

            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                // Convert each pair of hex chars to byte
                byte.TryParse(hex.Substring(i * 2, 2),
                     System.Globalization.NumberStyles.HexNumber,
                     null, out bytes[i]);
            }

            return bytes;
        }

        private void Parse(string macOriginal, string aesKey)
        {
            byte[] mac = StringToByteArray(macOriginal.Replace(":", ""));

            byte[] xiaomiMac = mac;
            byte[] associatedData = new byte[] { 0x11 };
            byte[] nonce = xiaomiMac.Reverse().Concat(_data.Skip(2).Take(3)).Concat(_data.Skip(_data.Length - 7).Take(3)).ToArray();
            byte[] mic = _data.Skip(_data.Length - 4).Take(4).ToArray();
            int i = 5;
            byte[] encryptedPayload = _data.Skip(i).Take(_data.Length - i - 7).ToArray();

            byte[] bindKey = StringToByteArray(aesKey);

            // AES-CCM decryption
            CcmBlockCipher ccm = new CcmBlockCipher(new Org.BouncyCastle.Crypto.Engines.AesEngine());
            AeadParameters parameters = new AeadParameters(new KeyParameter(bindKey), 32, nonce, associatedData);
            ccm.Init(false, parameters);

            byte[] cipherText = encryptedPayload.Concat(mic).ToArray();
            byte[] decryptedPayload = new byte[ccm.GetOutputSize(cipherText.Length)];
            int len = ccm.ProcessBytes(cipherText, 0, cipherText.Length, decryptedPayload, 0);
            ccm.DoFinal(decryptedPayload, len);

            byte[] obj = new byte[9]; // 12 - 3 = 9 bytes
            Array.Copy(decryptedPayload, 3, obj, 0, 9);

            // Extract bytes 1 to 4 from obj (4 bytes)
            byte[] slice = new byte[4];
            Array.Copy(obj, 1, slice, 0, 4);

            // Convert the 4 bytes to an integer (little-endian)
            int value = BitConverter.ToInt32(slice, 0);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(slice);
                value = BitConverter.ToInt32(slice, 0);
            }

            this.ParseValue(value);
        }

        private void ParseValue(int value)
        {
            var dict = new Dictionary<string, double>();
            var mass = (value & 0x7FF);
            var heartRate = (value >> 11) & 0x7F;
            var impedance = (value >> 18);


            if (mass != 0)
            {
                double weight = (value & 0x7FF) / 10.0;
                _weight = weight;
                this.UpdateSensor(Weight, weight);
            }
            if (0 < heartRate && heartRate < 127)
            {
                this.UpdateSensor(HeartRate, heartRate + 50);
            }
            if (impedance != 0)
            {
                if (mass != 0)
                {
                    _impedance = impedance / 10.0;
                    this.UpdateSensor(Impedance, impedance / 10.0);
                }
                else
                {
                    this.UpdateSensor(ImpedanceLow, impedance / 10);
                }
            }
        }

        public BodyComposition GetBodyComposition(User userInfo, S400InputData inputData)
        {
            _height = userInfo.Height;
            _age = userInfo.Age;
            _sex = userInfo.Sex;
            _data = inputData.Data is null ? StringToByteArray(inputData.DataString) : inputData.Data;

            if (!CheckInput(userInfo))
            {
                return null;
            }

            this.Parse(inputData.MacOriginal, inputData.AesKey);

            if (sensors.ContainsKey(Weight) && sensors[Weight] != 0)
            {
                var bc = GetBodyComposition();
                return bc;
            }
            return null;

        }

        private BodyComposition GetBodyComposition()
        {
            var bodyType = this.GetBodyType();

            var bodyComposition = new BodyComposition
            {
                Weight = GetSensorValue(Weight) ?? 0,
                BMI = Math.Round(this.GetBmi(), 1),
                ProteinPercentage = Math.Round(this.GetProteinPercentage(), 1),
                IdealWeight = Math.Round(this.GetIdealWeight(), 2),
                BMR = Math.Round(this.GetBmr(), 0),
                BoneMass = Math.Round(this.GetBoneMass(), 2),
                Fat = Math.Round(this.GetFatPercentage(), 1),
                MetabolicAge = Math.Round(this.GetMetabolicAge(), 0),
                MuscleMass = Math.Round(this.GetMuscleMass(), 2),
                VisceralFat = Math.Round(this.GetVisceralFat(), 2),
                Water = Math.Round(this.GetWater(), 1),
                BodyType = bodyType + 1,
                BodyTypeName = BodyTypeScale[bodyType],
                Date = DateTime.Now,
                Impedance = GetSensorValue(Impedance),
                HeartRate = GetSensorValue(HeartRate),

            };

            return bodyComposition;
        }

        private static MuscleMassScale[] MuscleMassScales => new[]
{
            new MuscleMassScale
            {
                Min = new Dictionary<Sex, double>() {{Sex.Male, 170}, {Sex.Female, 160}},
                Female = new[] {36.5, 42.6},
                Male = new[] {49.4, 59.5}
            },
            new MuscleMassScale
            {
                Min = new Dictionary<Sex, double>() {{Sex.Male, 160}, {Sex.Female, 150}},
                Female = new[] {32.9, 37.6},
                Male = new[] {44.0, 52.5}
            },
            new MuscleMassScale
            {
                Min = new Dictionary<Sex, double>() {{Sex.Male, 0}, {Sex.Female, 0}},
                Female = new[] {29.1, 34.8},
                Male = new[] {38.5, 46.6}
            }
        };

        //The included tables where quite strange, maybe bogus, replaced them with better ones...
        private static FatPercentageScale[] FatPercentageScales => new[]
        {
            new FatPercentageScale
            {
                Min = 0, Max = 12,
                Female = new double[] {12, 21, 30, 34},
                Male = new double[] {7, 16, 25, 30}
            },
            new FatPercentageScale
            {
                Min = 12, Max = 14,
                Female = new double[] {15.0, 24.0, 33.0, 37.0},
                Male = new double[] {7.0, 16.0, 25.0, 30.0},
            },
            new FatPercentageScale
            {
                Min = 14, Max = 16,
                Female = new double[] {18.0, 27.0, 36.0, 40.0},
                Male = new double[] {7.0, 16.0, 25.0, 30.0},
            },
            new FatPercentageScale
            {
                Min = 16, Max = 18,
                Female = new double[] {20.0, 28.0, 37.0, 41.0},
                Male = new double[] {7.0, 16.0, 25.0, 30.0},
            },
            new FatPercentageScale
            {
                Min = 18, Max = 40,
                Female = new double[] {21.0, 28.0, 35.0, 40.0},
                Male = new double[] {11.0, 17.0, 22.0, 27.0},
            },
            new FatPercentageScale
            {
                Min = 40, Max = 60,
                Female = new double[] {22.0, 29.0, 36.0, 41.0},
                Male = new double[] {12.0, 18.0, 23.0, 28.0},
            },
            new FatPercentageScale
            {
                Min = 60, Max = 100,
                Female = new double[] {23.0, 30.0, 37.0, 42.0},
                Male = new double[] {14.0, 20.0, 25.0, 30.0},
            }
        };

        private static string[] BodyTypeScale => new[]
        {
            "obese", "overweight", "thick-set", "lack-exerscise", "balanced", "balanced-muscular", "skinny",
            "balanced-skinny", "skinny-muscular"
        };

        private bool CheckInput(User userInfo)
        {
            if (userInfo is null)
            {
                throw new ArgumentNullException(nameof(userInfo), "information about user cannot be empty");
            }

            if (_data is null)
            {
                throw new ArgumentNullException(nameof(_data), "data cannot be empty");
            }

            if (_data.Length == (bufferSize + 2))
            {
                byte[] fixedData = new byte[_data.Length - 2];
                Array.Copy(_data, 2, fixedData, 0, fixedData.Length);
                _data = fixedData;
            }

            if (_data.Length != bufferSize)
            {
                return false;
            }

            return true;
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

        private double GetWater()
        {
            double coefficient;
            double waterPercentage = (100 - this.GetFatPercentage()) * 0.7;

            if (waterPercentage <= 50)
            {
                coefficient = 1.02;
            }
            else
            {
                coefficient = 0.98;
            }

            // Capping water percentage
            if (waterPercentage * coefficient >= 65)
            {
                waterPercentage = 75;
            }

            return CheckValueOverflow(waterPercentage * coefficient, 35, 75);
        }

        private int GetBodyType()
        {
            int factor;
            if (this.GetFatPercentage() > this.GetFatPercentageScale()[2])
            {
                factor = 0;
            }
            else if (this.GetFatPercentage() < this.GetFatPercentageScale()[1])
            {
                factor = 2;
            }
            else
            {
                factor = 1;
            }


            if (this.GetMuscleMass() > this.GetMuscleMassScale()[1])
            {
                return 2 + (factor * 3);
            }
            else if (this.GetMuscleMass() < this.GetMuscleMassScale()[0])
            {
                return (factor * 3);
            }
            else
            {
                return 1 + (factor * 3);
            }
        }

        private double GetIdealWeight()
        {
            switch (_sex)
            {
                case Sex.Male:
                    return (this._height - 80) * 0.7;
                case Sex.Female:
                    return (this._height - 70) * 0.6;
                default:
                    throw new NotImplementedException();
            }
        }

        private double GetMetabolicAge()
        {
            double metabolicAge;
            switch (_sex)
            {
                case Sex.Male:
                    metabolicAge = (this._height * -0.7471) + (this._weight * 0.9161) + (this._age * 0.4184) +
                                   (this._impedance * 0.0517) + 54.2267;
                    break;
                case Sex.Female:
                    metabolicAge = (this._height * -1.1165) + (this._weight * 1.5784) + (this._age * 0.4615) +
                                   (this._impedance * 0.0415) + 83.2548;
                    break;
                default:
                    throw new NotImplementedException();
            }

            return CheckValueOverflow(metabolicAge, 15, 80);
        }

        private double GetVisceralFat()
        {
            double subcalc, vfal;

            if (this._sex == Sex.Female)
            {
                if (this._weight > (13 - (this._height * 0.5)) * -1)
                {
                    var subsubcalc = ((this._height * 1.45) + (this._height * 0.1158) * this._height) - 120;
                    subcalc = this._weight * 500 / subsubcalc;
                    vfal = (subcalc - 6) + (this._age * 0.07);
                }
                else
                {
                    subcalc = 0.691 + (this._height * -0.0024) + (this._height * -0.0024);
                    vfal = (((this._height * 0.027) - (subcalc * this._weight)) * -1) + (this._age * 0.07) - this._age;
                }
            }
            else if (this._height < this._weight * 1.6)
            {
                subcalc = ((this._height * 0.4) - (this._height * (this._height * 0.0826))) * -1;
                vfal = ((this._weight * 305) / (subcalc + 48)) - 2.9 + (this._age * 0.15);
            }
            else
            {
                subcalc = 0.765 + this._height * -0.0015;
                vfal = (((this._height * 0.143) - (this._weight * subcalc)) * -1) + (this._age * 0.15) - 5.0;
            }

            return this.CheckValueOverflow(vfal, 1, 50);
        }

        private double GetProteinPercentage()
        {
            var proteinPercentage = (this.GetMuscleMass() / this._weight) * 100;
            proteinPercentage -= this.GetWaterPercentage();

            return this.CheckValueOverflow(proteinPercentage, 5, 32);
        }

        private double GetWaterPercentage()
        {
            var waterPercentage = (100 - this.GetFatPercentage()) * 0.7;
            var coefficient = 0.98;
            if (waterPercentage <= 50) coefficient = 1.02;
            if (waterPercentage * coefficient >= 65) waterPercentage = 75;
            return this.CheckValueOverflow(waterPercentage * coefficient, 35, 75);
        }

        private double GetBmi()
        {
            return this.CheckValueOverflow(this._weight / ((this._height / 100) * (this._height / 100)), 10, 90);
        }

        private double GetBmr()
        {
            double bmr;

            switch (this._sex)
            {
                case Sex.Male:
                    bmr = 877.8 + this._weight * 14.916;
                    bmr -= this._height * 0.726;
                    bmr -= this._age * 8.976;
                    if (bmr > 2322)
                    {
                        bmr = 5000;
                    }

                    break;
                case Sex.Female:
                    bmr = 864.6 + this._weight * 10.2036;
                    bmr -= this._height * 0.39336;
                    bmr -= this._age * 6.204;

                    if (bmr > 2996)
                    {
                        bmr = 5000;
                    }

                    break;
                default:
                    throw new NotImplementedException();
            }

            return this.CheckValueOverflow(bmr, 500, 10000);
        }

        private double GetFatPercentage()
        {
            var value = 0.8;
            if (this._sex == Sex.Female && this._age <= 49)
            {
                value = 9.25;
            }
            else if (this._sex == Sex.Female && this._age > 49)
            {
                value = 7.25;
            }

            var LBM = this.GetLbmCoefficient();
            var coefficient = 1.0;

            if (this._sex == Sex.Male && this._weight < 61)
            {
                coefficient = 0.98;
            }
            else if (this._sex == Sex.Female && this._weight > 60)
            {
                if (this._height > 160)
                {
                    coefficient *= 1.03;
                }
                else
                {
                    coefficient = 0.96;
                }
            }
            else if (this._sex == Sex.Female && this._weight < 50)
            {
                if (this._height > 160)
                {
                    coefficient *= 1.03;
                }
                else
                {
                    coefficient = 1.02;
                }
            }

            var fatPercentage = (1.0 - (((LBM - value) * coefficient) / this._weight)) * 100;


            if (fatPercentage > 63)
            {
                fatPercentage = 75;
            }

            return this.CheckValueOverflow(fatPercentage, 5, 75);
        }

        private double GetMuscleMass()
        {
            var muscleMass = this._weight - ((this.GetFatPercentage() * 0.01) * this._weight) - this.GetBoneMass();
            if (this._sex == Sex.Female && muscleMass >= 84)
            {
                muscleMass = 120;
            }
            else if (this._sex == Sex.Male && muscleMass >= 93.5)
            {
                muscleMass = 120;
            }

            return this.CheckValueOverflow(muscleMass, 10, 120);
        }

        private double GetBoneMass()
        {
            var @base = 0.18016894;
            if (this._sex == Sex.Female)
            {
                @base = 0.245691014;
            }

            var boneMass = (@base - (this.GetLbmCoefficient() * 0.05158)) * -1;

            if (boneMass > 2.2)
            {
                boneMass += 0.1;
            }
            else
            {
                boneMass -= 0.1;
            }

            if (this._sex == Sex.Female && boneMass > 5.1)
            {
                boneMass = 8;
            }
            else if (this._sex == Sex.Male && boneMass > 5.2)
            {
                boneMass = 8;
            }

            return this.CheckValueOverflow(boneMass, 0.5, 8);
        }

        private double GetLbmCoefficient()
        {
            var lbm = (this._height * 9.058 / 100) * (this._height / 100);
            lbm += this._weight * 0.32 + 12.226;
            lbm -= this._impedance * 0.0068;
            lbm -= this._age * 0.0542;
            return lbm;
        }

        private double[] GetMuscleMassScale()
        {
            var scale = MuscleMassScales.FirstOrDefault(s => this._height >= s.Min[this._sex]);

            switch (_sex)
            {
                case Sex.Female:
                    return scale.Female;
                case Sex.Male:
                    return scale.Male;
                default:
                    throw new NotImplementedException();
            }
        }

        private double[] GetFatPercentageScale()
        {
            var scale = FatPercentageScales
                .FirstOrDefault(s =>
                    this._age >= s.Min
                    && this._age < s.Max);

            switch (_sex)
            {
                case Sex.Female:
                    return scale.Female;
                case Sex.Male:
                    return scale.Male;
                default:
                    throw new NotImplementedException();
            }
        }

    }
}

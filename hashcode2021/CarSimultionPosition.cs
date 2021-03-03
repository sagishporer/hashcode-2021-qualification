using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace hashcode2021
{   
    class CarSimultionPosition
    {
        public Car Car;
        public int StreetNumber;
        public int TimeGotHere;
        public int TimeLeftOnDrive;

        public CarSimultionPosition(Car car, int timeGotHere)
        {
            this.Car = car;
            this.StreetNumber = 0;
            this.TimeGotHere = timeGotHere;
            this.TimeLeftOnDrive = car.TimeNeedToDrive();
        }

        public Street GetCurrentStreet()
        {
            return this.Car.Streets[this.StreetNumber];
        }
    }

    class CarSimultionPositionByTimeGotHere : IComparer<CarSimultionPosition>
    {
        public int Compare([AllowNull] CarSimultionPosition x, [AllowNull] CarSimultionPosition y)
        {
            return x.TimeGotHere.CompareTo(y.TimeGotHere);
        }
    }

    class CarSimultionPositionByTimeLeft : IComparer<CarSimultionPosition>
    {
        public int Compare([AllowNull] CarSimultionPosition x, [AllowNull] CarSimultionPosition y)
        {
            return x.TimeLeftOnDrive.CompareTo(y.TimeLeftOnDrive);
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using VMS.TPS.Common.Model.API;

namespace PlanCompare_SR_DB {
    public class FieldData : INotifyPropertyChanged {

        private string _fieldId;
        public string fieldId {
            get { return this._fieldId; }
            set {
                if (this._fieldId != value) {
                    this._fieldId = value;
                    this.NotifyPropertyChanged(nameof(fieldId));
                }
            }
        }

        private double _gantryAngle;
        public double gantryAngle {
            get { return this._gantryAngle; }
            set {
                if (this._gantryAngle != value) {
                    this._gantryAngle = value;
                    this.NotifyPropertyChanged(nameof(gantryAngle));
                }
            }
        }

        private double _collAngle;
        public double collAngle {
            get { return this._collAngle; }
            set {
                if (this._collAngle != value) {
                    this._collAngle = value;
                    this.NotifyPropertyChanged(nameof(collAngle));
                }
            }
        }

        private double _tableAngle;
        public double tableAngle {
            get { return this._tableAngle; }
            set {
                if (this._tableAngle != value) {
                    this._tableAngle = value;
                    this.NotifyPropertyChanged(nameof(tableAngle));
                }
            }
        }

        private double _X1;
        public double X1 {
            get { return this._X1; }
            set {
                if (this._X1 != value) {
                    this._X1 = value;
                    this.NotifyPropertyChanged(nameof(X1));
                }
            }
        }

        private double _X2;
        public double X2 {
            get { return this._X2; }
            set {
                if (this._X2 != value) {
                    this._X2 = value;
                    this.NotifyPropertyChanged(nameof(X2));
                }
            }
        }

        private double _Y1;
        public double Y1 {
            get { return this._Y1; }
            set {
                if (this._Y1 != value) {
                    this._Y1 = value;
                    this.NotifyPropertyChanged(nameof(Y1));
                }
            }
        }

        private double _Y2;
        public double Y2 {
            get { return this._Y2; }
            set {
                if (this._Y2 != value) {
                    this._Y2 = value;
                    this.NotifyPropertyChanged(nameof(Y2));
                }
            }
        }

        private double _MUs;
        public double MUs {
            get { return this._MUs; }
            set {
                if (this._MUs != value) {
                    this._MUs = value;
                    this.NotifyPropertyChanged(nameof(MUs));
                }
            }
        }

        private double _fieldDose;
        public double fieldDose {
            get { return this._fieldDose; }
            set {
                if (this._fieldDose != value) {
                    this._fieldDose = value;
                    this.NotifyPropertyChanged(nameof(fieldDose));
                }
            }
        }


        //Constructor
        public FieldData(Beam theBeam)
        {
            if(theBeam != null) {
                fieldId = theBeam.Id;
                gantryAngle = theBeam.ControlPoints[0].GantryAngle;
                collAngle = theBeam.ControlPoints[0].CollimatorAngle;
                tableAngle = theBeam.ControlPoints[0].PatientSupportAngle;
                X1 = theBeam.ControlPoints[0].JawPositions.X1;
                X2 = theBeam.ControlPoints[0].JawPositions.X2;
                Y1 = theBeam.ControlPoints[0].JawPositions.Y1;
                Y2 = theBeam.ControlPoints[0].JawPositions.Y2;
                MUs = theBeam.Meterset.Value;
                fieldDose = (MUs / theBeam.MetersetPerGy);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null) {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

    }
}

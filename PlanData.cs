using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace PlanCompare_SR_DB {
    public class PlanData : INotifyPropertyChanged {

        private string _planId;
        public string planId { 
            get { return this._planId; }
            set { 
                if (this._planId != value) {
                    this._planId = value;
                    this.NotifyPropertyChanged(nameof(planId));
                }
            } 
        }

        private int _numOfFields;
        public int numOfFields {
            get { return this._numOfFields; }
            set {
                if (this._numOfFields != value) {
                    this._numOfFields = value;
                    this.NotifyPropertyChanged(nameof(numOfFields));
                }
            }
        }

        private string _algorithm;
        public string algorithm {
            get { return this._algorithm; }
            set {
                if (this._algorithm != value) {
                    this._algorithm = value;
                    this.NotifyPropertyChanged(nameof(algorithm));
                }
            }
        }

        private double _dosePerFx;
        public double dosePerFx {
            get { return this._dosePerFx; }
            set {
                if (this._dosePerFx != value) {
                    this._dosePerFx = value;
                    this.NotifyPropertyChanged(nameof(dosePerFx));
                }
            }
        }

        private int _numOfFx;
        public int numOfFx {
            get { return this._numOfFx; }
            set {
                if (this._numOfFx != value) {
                    this._numOfFx = value;
                    this.NotifyPropertyChanged(nameof(numOfFx));
                }
            }
        }

        private double _totalRxDose;
        public double totalRxDose {
            get { return this._totalRxDose; }
            set {
                if (this._totalRxDose != value) {
                    this._totalRxDose = value;
                    this.NotifyPropertyChanged(nameof(totalRxDose));
                }
            }
        }

        private double _maxDose;
        public double maxDose {
            get { return this._maxDose; }
            set {
                if (this._maxDose != value) {
                    this._maxDose = value;
                    this.NotifyPropertyChanged(nameof(maxDose));
                }
            }
        }

        private string _targVol;
        public string targVol {
            get { return this._targVol; }
            set {
                if (this._targVol != value) {
                    this._targVol = value;
                    this.NotifyPropertyChanged(nameof(targVol));
                }
            }
        }

        private double _targMaxDose;
        public double targMaxDose {
            get { return this._targMaxDose; }
            set {
                if (this._targMaxDose != value) {
                    this._targMaxDose = value;
                    this.NotifyPropertyChanged(nameof(targMaxDose));
                }
            }
        }

        private double _targMinDose;
        public double targMinDose {
            get { return this._targMinDose; }
            set {
                if (this._targMinDose != value) {
                    this._targMinDose = value;
                    this.NotifyPropertyChanged(nameof(targMinDose));
                }
            }
        }

        private double _targMeanDose;
        public double targMeanDose {
            get { return this._targMeanDose; }
            set {
                if (this._targMeanDose != value) {
                    this._targMeanDose = value;
                    this.NotifyPropertyChanged(nameof(targMeanDose));
                }
            }
        }

        private List<FieldData> _fields;
        public List<FieldData> fields {
            get { return this._fields; }
            set {
                if (this._fields != value) {

                    if(this._fields != null) {
                        foreach(FieldData fd in _fields) {
                            if(fd != null) { fd.PropertyChanged -= ChildPropertyChanged; }
                        }
                    }

                    this._fields = value;

                    if (this._fields != null) {
                        foreach (FieldData fd in _fields) {
                            if (fd != null) { fd.PropertyChanged += ChildPropertyChanged; }
                        }
                    }

                    this.NotifyPropertyChanged(nameof(fields));


                }
                void ChildPropertyChanged(object sender, PropertyChangedEventArgs args)
                {
                    NotifyPropertyChanged("");
                }
            }
        }



        //Constructor
        public PlanData(PlanSetup aPlanSetup)
        {
            planId = aPlanSetup.Id;
            numOfFields = aPlanSetup.Beams.Count();
            algorithm = aPlanSetup.PhotonCalculationModel;
            dosePerFx = aPlanSetup.DosePerFraction.Dose;
            numOfFx = aPlanSetup.NumberOfFractions.Value;
            totalRxDose = aPlanSetup.TotalDose.Dose;
                
            targVol = aPlanSetup.TargetVolumeID.ToString();

            if(aPlanSetup.Dose !=null) {
                maxDose = aPlanSetup.Dose.DoseMax3D.Dose;
                Structure tVol = GetStructureByName(aPlanSetup, targVol);
                targMaxDose = aPlanSetup.GetDoseAtVolume(tVol, 0, VolumePresentation.Relative, DoseValuePresentation.Absolute).Dose;
                targMinDose = aPlanSetup.GetDoseAtVolume(tVol, 100, VolumePresentation.Relative, DoseValuePresentation.Absolute).Dose;
                targMeanDose = aPlanSetup.GetDoseAtVolume(tVol, 50, VolumePresentation.Relative, DoseValuePresentation.Absolute).Dose;
            }
            else {
                maxDose = 0;
                Structure tVol = GetStructureByName(aPlanSetup, targVol);
                targMaxDose = 0;
                targMinDose = 0;
                targMeanDose = 0;
            }


            if(fields != null) { fields.Clear(); }
            fields = new List<FieldData>();
            fields.Add(new FieldData(null) );  //Add a blank field data so that indexes for actual fields start at 1
            foreach(Beam aBeam in aPlanSetup.Beams) {
                fields.Add(new FieldData(aBeam));
            }
        }

        //Utility function to get a structure object by name string
        public Structure GetStructureByName(PlanSetup aPlan, string structName)
        {
            foreach (Structure aStruct in aPlan.StructureSet.Structures) {
                if (aStruct.Id == structName) {
                    return aStruct;
                }
            }
            return null;
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

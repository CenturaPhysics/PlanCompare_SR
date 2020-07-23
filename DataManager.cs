using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;


namespace PlanCompare_SR_DB {
    public class DataManager : INotifyPropertyChanged {

        //The overall data items for storing the plan data.  Setup as arrays, ignoring the zero index, so we can refer to plans
        //using index 1 and 2.

        public class MyCourseCollection : ObservableCollection<Course> {
            public DataManager parent { get; set; }

            protected override void SetItem(int index, Course item)
            {
                base.SetItem(index, item);
                if(item != null && parent != null) { 
                    parent.courseIDs[index] = item.Id.ToString(); 
                }
            }
        }

        private MyCourseCollection _theCourses;
        public MyCourseCollection theCourses {
            get { return _theCourses; }
            set {
                if (_theCourses != value) {
                    _theCourses = value;
                    _theCourses.parent = this;
                    NotifyPropertyChanged(nameof(theCourses));
                }
            }
        }

        private ObservableCollection<string> _courseIDs;
        public ObservableCollection<string> courseIDs {
            get { return _courseIDs; }
            set {
                if (_courseIDs != value) {
                    _courseIDs = value;
                    NotifyPropertyChanged(nameof(courseIDs));
                }
            }
        }


        private PlanData[] _thePlans;
        public PlanData[] thePlans {
            get { return _thePlans; }
            set {
                if(_thePlans != value) {
                    
                    if( _thePlans != null) {
                        for (int i = 0; i < _thePlans.Length; i++) {
                            if(_thePlans[i] != null) { _thePlans[i].PropertyChanged -= ChildPropertyChanged; }
                        }
                    }

                    _thePlans = value;

                    if (_thePlans != null) {
                        for (int i = 0; i < _thePlans.Length; i++) {
                            if (_thePlans[i] != null) { _thePlans[i].PropertyChanged += ChildPropertyChanged; }
                        }
                    }

                    NotifyPropertyChanged("thePlans");
                }

                void ChildPropertyChanged(object sender, PropertyChangedEventArgs args)
                {
                    NotifyPropertyChanged("");
                }
            }
        }


        //Properties to hold data objects for artificial context for ScriptRunner.  Not needed for PlugIn version.
        //Will need to add 'sr_' to data bindings in the main window xaml code.
        //REMOVE BELOW FOR PLUGIN APP
        private Patient _sr_Patient;
        public Patient sr_Patient {
            get { return this._sr_Patient; }
            set {
                if (this._sr_Patient != value) {
                    this._sr_Patient = value;
                    this.NotifyPropertyChanged(nameof(sr_Patient));
                }
            }
        }

        private PlanSetup _sr_PlanSetup;
        public PlanSetup sr_PlanSetup {
            get { return this._sr_PlanSetup; }
            set {
                if (this._sr_PlanSetup != value) {
                    this._sr_PlanSetup = value;
                    this.NotifyPropertyChanged(nameof(sr_PlanSetup));
                }
            }
        }
        //REMOVE ABOVE FOR PLUGIN APP

        private double _passingThreshold;
        public double passingThreshold {
            get { return this._passingThreshold; }
            set {
                if (this._passingThreshold != value) {
                    this._passingThreshold = value;
                    this.NotifyPropertyChanged(nameof(passingThreshold));
                }
            }
        }

        //property and fucntion for providing notification of data changes, for dynamic data binding.
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null) {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }


        //Class to hold data for plan comparisons.  We make these so we can more easily iterate through for plan comparisons.  
        //The General field info is a instance of this class.
        //The Field info is a List of instances of this class.
        public class PlanCompareLists {
            public int count;
            public bool total_bool;  //the cumulative boolean of the entire list - used mostly for the field lists
            public List<CLDataInfo> dataInfo;
            public List<CompareListItem> plan1List;
            public List<CompareListItem> plan2List;
            public List<ResultsItems> resultList;

            //Constructor
            public PlanCompareLists()
            {
                count = 0;
                total_bool = true;
                dataInfo = new List<CLDataInfo>();
                plan1List = new List<CompareListItem>();
                plan2List = new List<CompareListItem>();
                resultList = new List<ResultsItems>();
            }
        }

        //The following data structures (class objects) are setup for storing data comparison info.  Each of these are stored as a 
        //list in the PlanCompareList class

            //Data structure to hold information about the data comparison items.
            public class CLDataInfo {
                public int dataType;      //use 1=double, 2=string
                public string dataTag;    //used to indentify the held data.  Currently unused, but set to a string describing the data item.

                public CLDataInfo(int aDataType, string aDataTag)
                {
                    dataType = aDataType;
                    dataTag = aDataTag;
                }
            }

            //Data structure for the storage of the actual comparison data, either double or string
            public class CompareListItem {
                public double numData;    //used to store numeric data.  Set to -999 if unused.
                public string stringData; //used to store string data.  Set to "" if unused.

                public CompareListItem(double aNumData, string aStringData) 
                {
                    numData = aNumData;
                    stringData = aStringData;
                }
            }

            //Data structure to hold information about the results of a data comparison
            public class ResultsItems {
                public double passThreshold;
                public bool result;
                public string cbName;
                public SolidColorBrush brush;

                public ResultsItems(string aCBName, double aPassThreshold)
                {
                    passThreshold = aPassThreshold;
                    result = true;
                    cbName = aCBName;
                    brush = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                }
            }


        public PlanCompareLists clGeneral;
        public List<PlanCompareLists> clFields;

        public TextBox debugTB;

        // * * * Constructor * * *
        public DataManager(PlanSetup aPlan)
        {
            courseIDs = new ObservableCollection<string>() { "", "", "" };
            theCourses = new MyCourseCollection() { null, null, null };

            thePlans = new PlanData[3];
            thePlans[0] = null;
            thePlans[1] = null;
            thePlans[2] = null;

            clGeneral = new PlanCompareLists();
            clFields = new List<PlanCompareLists>();

            passingThreshold = 1;
        }


        //Utility function to get a structure object by name string
        public Structure GetStructureByName(PlanSetup aPlan, string structName)
        {
            foreach (Structure aStruct in aPlan.StructureSet.Structures) {
                if(aStruct.Id == structName) {
                    return aStruct;
                }
            }
            return null;
        }


        //Utility function for above LoadDataForPlan.  Allows the adding of a TextBlock to a grid cell in one function.
        public void AddTextBlockToFieldGridAt(Grid aGrid, TextBlock aTB, int aWidth, int aRow, int aCol)
        {
            aTB.FontSize = 12;
            aTB.Width = aWidth;
            aTB.TextAlignment = TextAlignment.Center;
            aTB.Margin = new Thickness(0, 4, 0, 4);
            aGrid.Children.Add(aTB);
            Grid.SetRow(aTB, aRow);
            Grid.SetColumn(aTB, aCol);
        }


        //Function for adding plan comparison info to the PlanCompareLists item specified by the plan number.  The General Info
        //is simply a list of CompareListItems.  The Field info is a nested list of lists of CompareListItems, where we add a new 
        //List of CompareListItems for each field.  Thus, to access the comparison data, we have:  planCompLists[planNum].cfields[fieldNum]
        public void SetCompareLists()
        {

            //Before continuing, ensure that a plan has been stored in the data manager array of plans for each plan.
            if (thePlans[1] != null && thePlans[2] != null) {
                //Only continue if the number of fields for both plans is equal
                if(thePlans[1].fields.Count() == thePlans[2].fields.Count()) {
                    //Clear the current plan comparison data, so that we can add new fresh data.
                    ClearCompareLists();
                    int fldCount = thePlans[1].numOfFields;

                    //Add an initial result to the General lists.  This is item 0, and will be used to display the All-Fields result.
                    //Note that both the string added is blank and the number added is -999, as this is just a placeholder.
                    AddPlanCompItem(clGeneral, 2, "FieldSummary", -999, "", -999, "", "comp_all_flds_okay", -1);

                    //Add the General field info...
                    AddPlanCompItem(clGeneral, 1, "NumOfFields", thePlans[1].numOfFields, "", thePlans[2].numOfFields, "", "compNumOfFields", -1);
                    AddPlanCompItem(clGeneral, 2, "Fractionation", thePlans[1].dosePerFx, "", thePlans[2].dosePerFx, "", "compFx", -1);
                    AddPlanCompItem(clGeneral, 2, "Fractionation", thePlans[1].numOfFx, "", thePlans[2].numOfFx, "", "compFx", -1);
                    AddPlanCompItem(clGeneral, 2, "Fractionation", thePlans[1].totalRxDose, "", thePlans[2].totalRxDose, "", "compFx", -1);
                    AddPlanCompItem(clGeneral, 1, "MaxDose", thePlans[1].maxDose, "", thePlans[2].maxDose, "", "compMaxDose", passingThreshold);
                    AddPlanCompItem(clGeneral, 2, "TargetVol", -999, thePlans[1].targVol, -999, thePlans[2].targVol, "compTargVol", -1);
                    AddPlanCompItem(clGeneral, 1, "MaxTargDose", thePlans[1].targMaxDose, "", thePlans[2].targMaxDose, "", "compMaxTargDose", passingThreshold);
                    AddPlanCompItem(clGeneral, 1, "MinTargDose", thePlans[1].targMinDose, "", thePlans[2].targMinDose, "", "compMinTargDose", passingThreshold);
                    AddPlanCompItem(clGeneral, 1, "MeanTargDose", thePlans[1].targMeanDose, "", thePlans[2].targMeanDose, "", "compMeanTargDose", passingThreshold);

                    if (thePlans[1].fields.Count() == thePlans[2].fields.Count()) {
                        //For each field, add a list of CompareListItems.  Then, add CompareListItems to each inner list for each field parameter.
                        //Also add an entry to the list of field comparison results
                        for (int i = 0; i < fldCount; i++) {
                            FieldData P1_f = thePlans[1].fields[i];
                            FieldData P2_f = thePlans[2].fields[i];
                            string fldNumStr = i.ToString();
                            clFields.Add(new PlanCompareLists());
                            AddPlanCompItem(clFields[i], 1, "F" + fldNumStr + ":GantryAngle", P1_f.gantryAngle, "", P2_f.gantryAngle, "", "compF" + i.ToString(), -1);
                            AddPlanCompItem(clFields[i], 1, "F" + fldNumStr + ":CollAngle", P1_f.collAngle, "", P2_f.collAngle, "", "compF" + i.ToString(), -1);
                            AddPlanCompItem(clFields[i], 1, "F" + fldNumStr + ":TableAngle", P1_f.tableAngle, "", P2_f.tableAngle, "", "compF" + i.ToString(), -1);
                            AddPlanCompItem(clFields[i], 1, "F" + fldNumStr + ":X1", P1_f.X1, "", P2_f.X1, "", "compF" + i.ToString(), -1);
                            AddPlanCompItem(clFields[i], 1, "F" + fldNumStr + ":X2", P1_f.X2, "", P2_f.X2, "", "compF" + i.ToString(), -1);
                            AddPlanCompItem(clFields[i], 1, "F" + fldNumStr + ":Y1", P1_f.Y1, "", P2_f.Y1, "", "compF" + i.ToString(), -1);
                            AddPlanCompItem(clFields[i], 1, "F" + fldNumStr + ":Y2", P1_f.Y2, "", P2_f.Y2, "", "compF" + i.ToString(), -1);
                        }
                    }
                }
            }
        }


        //Utility function for adding items to the comparelists.  Adding all of these at the same time ensures that our index is the same for 
        //all sublists in the PlanCompareLists class object for a given comparison item. 
        public void AddPlanCompItem(PlanCompareLists theList, int dataType, string dataTag, double P1_numData, string P1_stringData, double P2_numData, string P2_stringData, string cbName, double thePassThreshold)
        {
            theList.dataInfo.Add(new CLDataInfo(dataType, dataTag));
            theList.plan1List.Add(new CompareListItem(P1_numData, P1_stringData));
            theList.plan2List.Add(new CompareListItem(P2_numData, P2_stringData));
            //passingThreshold should be set to -1 for comparison items that are not using a passing threshold
            theList.resultList.Add(new ResultsItems(cbName, thePassThreshold));
            theList.count = theList.count + 1;
        }


        //Find the compare lists index for a particulare plan data, by the stored dataTag in the dataInfo list.
        public int GetCompareListIndexByTag(PlanCompareLists aPlanCompareListsObj, string aDataTag)
        {
            for (int i = 0; i < aPlanCompareListsObj.count; i++) {
                if(aPlanCompareListsObj.dataInfo[i].dataTag == aDataTag) {
                    return i;
                }
            }
            return -1;
        }


        //Clear the PlanCompareLists class of all data, including all the sub-lists.
        public void ClearCompareLists()
        {
            if (clGeneral.dataInfo != null) { clGeneral.dataInfo.Clear(); }
            if (clGeneral.plan1List != null) { clGeneral.plan1List.Clear(); }
            if (clGeneral.plan2List != null) { clGeneral.plan2List.Clear(); }
            if (clGeneral.resultList != null) { clGeneral.resultList.Clear(); }
            clGeneral.count = 0;

            foreach(PlanCompareLists pcList in clFields) {
                if (pcList.dataInfo != null) { pcList.dataInfo.Clear(); }
                if (pcList.plan1List != null) { pcList.plan1List.Clear(); }
                if (pcList.plan2List != null) { pcList.plan2List.Clear(); }
                if (pcList.resultList != null) { pcList.resultList.Clear(); }
                pcList.count = 0;
            }
            clFields.Clear();
        }


        //These two functions are for comparing plans. This one compares the general info.  Sets marker to red if doesn't match.
        //For field data, starts by assuming that the data for each field matches, and sets the field_okay marker
        //to green.  
        public void Check_General_Comparisons()
        {
            int compGenCnt = clGeneral.count;

            //Iterate through the general info.  Compare either numeric or string data.  Set the results list item to the boolean of whether the
            //two data items are equal.
            //Start from item 1, as item 0 is reserved for the all-fields result, and will be updated below.
            for (int i = 1; i < compGenCnt; i++) {
                if (clGeneral.dataInfo[i].dataType == 1) {
                    //Check to see if there is a passing threshold.  If there is, calc the percent difference, and see if that is
                    //greater than the threshold.  Otherwise, just do direct comparison of the values.  Pass threshold is set to "-1"
                    //for values that do NOT use a passing threshold.
                    if(clGeneral.resultList[i].passThreshold != -1) {
                        double diff = 100 * ((clGeneral.plan2List[i].numData - clGeneral.plan1List[i].numData) / clGeneral.plan1List[i].numData);
                        diff = Math.Abs(diff);
                        if(diff > clGeneral.resultList[i].passThreshold) {
                            clGeneral.resultList[i].result = false;
                        }
                        else {
                            clGeneral.resultList[i].result = true;
                        }

                    }
                    else {
                        clGeneral.resultList[i].result = clGeneral.plan1List[i].numData == clGeneral.plan2List[i].numData;
                    }
                    
                }
                if (clGeneral.dataInfo[i].dataType == 2) {
                    clGeneral.resultList[i].result = clGeneral.plan1List[i].stringData == clGeneral.plan2List[i].stringData;
                }
            }

        }

        //As with the above function, but this one itereates through the field list and sets the field_okay marker to red 
        //if it encounters a discrepancy.
        public string Check_Field_Comparisons()
        {
            int compFldCnt = clFields.Count();

            if (thePlans[1].numOfFields != thePlans[2].numOfFields) {
                MessageBox.Show("The two plans have different numbers of fields.  Field comparison will not be done.");
                return "FAILED";
            }
            else {
                //Iterate through the field info starting with index 1, since index 0 is reserved for representing the entire list.  
                //Compare either numeric or string data.  Set the results list item to the boolean of whether the two data items are equal and the 
                //logical AND of the current state of the field comparison boolean.  This way, if even one paramter compares to 'false', the result
                //for the entire field is set to 'false'.
                bool curAllFieldsBool = clGeneral.resultList[0].result;  //track the cumulative boolean for all fields
                bool curFieldBool = true;                               //track the cumulative boolean for all items for each field
                                                                        //debugTB.AppendText("Initial All-Fields Bool is" + curAllFieldsBool.ToString() + "\r\n");

                for (int i = 0; i < compFldCnt; i++) {
                    int fieldDataItems = clFields[i].count;

                    for (int j = 0; j < fieldDataItems; j++) {
                        if (clFields[i].dataInfo[j].dataType == 1) {
                            clFields[i].resultList[j].result = clFields[i].plan1List[j].numData == clFields[i].plan2List[j].numData;
                        }
                        if (clFields[i].dataInfo[j].dataType == 2) {
                            clFields[i].resultList[j].result = clFields[i].plan1List[j].stringData == clFields[i].plan2List[j].stringData;
                        }
                        curFieldBool = curFieldBool && clFields[i].resultList[j].result;
                    }
                    clFields[i].total_bool = curFieldBool;
                    curAllFieldsBool = curAllFieldsBool && curFieldBool;
                }
                clGeneral.resultList[0].result = curAllFieldsBool;
                return "OK";
            }
        }


        public void UpdatePlanDoseThresholds()
        {
            if (thePlans[1] != null && thePlans[2] != null) {
                SetCompareLists();
                foreach (ResultsItems rI in clGeneral.resultList) {
                    if (rI.passThreshold != -1) {
                        rI.passThreshold = passingThreshold;
                    }
                }
                Check_General_Comparisons();
                Check_Field_Comparisons();
            }
        }


    }

}

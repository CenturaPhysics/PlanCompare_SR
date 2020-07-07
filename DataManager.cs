using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace PlanCompare_SR {
    public class DataManager {

        //The overall data items for storing the plan data.  Setup as arrays, ignoring the zero index, so we can refer to plans
        //using index 1 and 2.
        public Course[] theCourses = new Course[3];
        public PlanSetup[] thePlans = new PlanSetup[3];


        //Properties to hold data objects for artificial context for ScriptRunner.  Not needed for PlugIn version.
        //Will need to add 'sr_' to data bindings in the main window xaml code.
        //REMOVE BELOW FOR PLUGIN APP
            private Patient _sr_Patient;
            public Patient sr_Patient {
                get { return this._sr_Patient; }
                set {
                    if (this._sr_Patient != value) {
                        this._sr_Patient = value;
                        this.NotifyPropertyChanged( nameof(sr_Patient) );
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
                public bool result;
                public string cbName;

                public ResultsItems(string aCBName)
                {
                    result = true;
                    cbName = aCBName;
                }
            }


        public PlanCompareLists clGeneral;
        public List<PlanCompareLists> clFields;

        public TextBox debugTB;

        //Constructor
        public DataManager()
        {
            theCourses[0] = null;
            theCourses[1] = null;
            theCourses[2] = null;

            thePlans[0] = null;
            thePlans[1] = null;
            thePlans[2] = null;

            clGeneral = new PlanCompareLists();
            clFields = new List<PlanCompareLists>();
        }


        //A function for presenting the plan data.  First it updates the plan info in the general section by accessing the TextBlocks
        //stored in the passed TextBlock list.  Next, it adds data to the field grid (passed as parameter) wih a column offset.
        //This function should be agnostic with respect to the interface... just updating the interface objects that are sent.
        //Last, it calls the SetCompareListForPlan function to update the lists of comparison items and results.
        //MessageBox.Show("Start of LoadDataForPlan(" + planNum.ToString() + ").");
        public void SetPlanData(int planNum, Grid aFieldGrid, List<TextBlock> genTextBlockList, List<TextBlock> fldTextBlockList, int gridColOffset) {
            if (theCourses[planNum] != null) {
                genTextBlockList[0].Text = theCourses[planNum].Id.ToString();

                if (thePlans[planNum] != null) {
                    //MessageBox.Show("Made it into the if statements of LoadDataForPlan(" + planNum.ToString() + ").");
                    PlanSetup aPlan = thePlans[planNum];

                    genTextBlockList[1].Text = aPlan.Id.ToString();
                    genTextBlockList[2].Text = aPlan.Beams.Count().ToString();
                    genTextBlockList[3].Text = aPlan.PhotonCalculationModel;
                    genTextBlockList[4].Text = aPlan.DosePerFraction.ToString();
                    genTextBlockList[5].Text = aPlan.NumberOfFractions.ToString();
                    genTextBlockList[6].Text = aPlan.TotalDose.ToString();

                    int fldCount = aPlan.Beams.Count();

                    for (int i = 0; i < fldCount; i++) {
                        Beam curBeam = aPlan.Beams.ElementAt(i);

                        TextBlock newTB = new TextBlock();
                        newTB.Text = curBeam.Id;
                        AddTextBlockToFieldGridAt(aFieldGrid, newTB, 65, 2 + i, gridColOffset + 1);
                        fldTextBlockList.Add(newTB);

                        newTB = new TextBlock();
                        newTB.Text = curBeam.ControlPoints[0].GantryAngle.ToString();
                        AddTextBlockToFieldGridAt(aFieldGrid, newTB, 50, 2 + i, gridColOffset + 2);
                        fldTextBlockList.Add(newTB);

                        newTB = new TextBlock();
                        newTB.Text = curBeam.ControlPoints[0].CollimatorAngle.ToString();
                        AddTextBlockToFieldGridAt(aFieldGrid, newTB, 50, 2 + i, gridColOffset + 3);
                        fldTextBlockList.Add(newTB);

                        newTB = new TextBlock();
                        newTB.Text = curBeam.ControlPoints[0].PatientSupportAngle.ToString();
                        AddTextBlockToFieldGridAt(aFieldGrid, newTB, 50, 2 + i, gridColOffset + 4);
                        fldTextBlockList.Add(newTB);

                        newTB = new TextBlock();
                        newTB.Text = curBeam.ControlPoints[0].JawPositions.X1.ToString();
                        AddTextBlockToFieldGridAt(aFieldGrid, newTB, 40, 2 + i, gridColOffset + 5);
                        fldTextBlockList.Add(newTB);

                        newTB = new TextBlock();
                        newTB.Text = curBeam.ControlPoints[0].JawPositions.X2.ToString();
                        AddTextBlockToFieldGridAt(aFieldGrid, newTB, 40, 2 + i, gridColOffset + 6);
                        fldTextBlockList.Add(newTB);

                        newTB = new TextBlock();
                        newTB.Text = curBeam.ControlPoints[0].JawPositions.Y1.ToString();
                        AddTextBlockToFieldGridAt(aFieldGrid, newTB, 40, 2 + i, gridColOffset + 7);
                        fldTextBlockList.Add(newTB);

                        newTB = new TextBlock();
                        newTB.Text = curBeam.ControlPoints[0].JawPositions.Y2.ToString();
                        AddTextBlockToFieldGridAt(aFieldGrid, newTB, 40, 2 + i, gridColOffset + 8);
                        fldTextBlockList.Add(newTB);

                        newTB = new TextBlock();
                        newTB.Text = string.Format("{0:0.0}", curBeam.Meterset.Value);
                        AddTextBlockToFieldGridAt(aFieldGrid, newTB, 40, 2 + i, gridColOffset + 9);
                        fldTextBlockList.Add(newTB);
                    }

                    string tVolId = aPlan.TargetVolumeID;
                    Structure tStruct;
                    DoseValue tMinVolDose;
                    DoseValue tMaxVolDose;

                    string tVolDoseStr;
                    foreach(Structure aStruct in aPlan.StructureSet.Structures) {
                        string tempStr = aStruct.Id;
                        if(aStruct.Id.ToString() == tVolId) {
                            tStruct = aStruct;
                            tMinVolDose = aPlan.GetDoseAtVolume(tStruct, 100, VolumePresentation.Relative, DoseValuePresentation.Relative);
                            tMaxVolDose = aPlan.GetDoseAtVolume(tStruct, 0, VolumePresentation.Relative, DoseValuePresentation.Relative);

                            tVolDoseStr = tMinVolDose.ValueAsString;
                        }
                    }

                    //MessageBox.Show("Just before checking if planNum is 2.  Current planNum = " + planNum.ToString() );
                    if (planNum == 2 && thePlans[1] != null) {
                        SetCompareLists();
                        Check_Comparison();
                    }
                }
            }
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


        //Function to clear all plan data.  Currently, just clears the plan comparison lists, but may be modified for additional
        //data clearing later.
        public void ClearPlanData(int planNum)
        {
            ClearCompareLists();
        }


        //Function for adding plan comparison info to the PlanCompareLists item specified by the plan number.  The General Info
        //is simply a list of CompareListItems.  The Field info is a nested list of lists of CompareListItems, where we add a new 
        //List of CompareListItems for each field.  Thus, to access the comparison data, we have:  planCompLists[planNum].cfields[fieldNum]
        public void SetCompareLists()
        {

            //Before continuing, ensure that a plan has been stored in the data manager array of plans for each plan.
            if (thePlans[1] != null && thePlans[2] != null) {
                //Clear the current plan comparison data, so that we can add new fresh data.
                ClearCompareLists();
                int fldCount = thePlans[1].Beams.Count();

                //Add an initial result to the General lists.  This is item 0, and will be used to display the All-Fields result.
                //Note that both the string added is blank and the number added is -999, as this is just a placeholder.
                AddPlanCompItem(clGeneral, 2, "FieldSummary", -999, "", -999, "", "comp_all_flds_okay");

                //Add the General field info...
                AddPlanCompItem(clGeneral, 1, "NumOfFields", thePlans[1].Beams.Count(), "", thePlans[2].Beams.Count(), "", "compNumOfFields");

                string fxString1 = thePlans[1].DosePerFraction.Dose.ToString() + " x ";
                fxString1 = fxString1 + thePlans[1].NumberOfFractions.ToString() + " = ";
                fxString1 = fxString1 + thePlans[1].TotalDose.Dose.ToString();
                string fxString2 = thePlans[2].DosePerFraction.Dose.ToString() + " x ";
                fxString2 = fxString2 + thePlans[2].NumberOfFractions.ToString() + " = ";
                fxString2 = fxString2 + thePlans[2].TotalDose.Dose.ToString();
                AddPlanCompItem(clGeneral, 2, "Fractionation", -999, fxString1, -999, fxString2, "compFx");


                //For each field, add a list of CompareListItems.  Then, add CompareListItems to each inner list for each field parameter.
                //Also add an entry to the list of field comparison results
                for (int i = 0; i < fldCount; i++) {
                    Beam P1_beam = thePlans[1].Beams.ElementAt(i);
                    Beam P2_beam = thePlans[2].Beams.ElementAt(i);
                    string fldNumStr = i.ToString();
                    clFields.Add( new PlanCompareLists());
                    AddPlanCompItem(clFields[i], 1, "F" + fldNumStr + ":GantryAngle", P1_beam.ControlPoints[0].GantryAngle, "", P2_beam.ControlPoints[0].GantryAngle, "", "compF" + i.ToString());
                    AddPlanCompItem(clFields[i], 1, "F" + fldNumStr + ":CollAngle", P1_beam.ControlPoints[0].CollimatorAngle, "", P2_beam.ControlPoints[0].CollimatorAngle, "", "compF" + i.ToString());
                    AddPlanCompItem(clFields[i], 1, "F" + fldNumStr + ":TableAngle", P1_beam.ControlPoints[0].PatientSupportAngle, "", P2_beam.ControlPoints[0].PatientSupportAngle, "", "compF" + i.ToString());
                    AddPlanCompItem(clFields[i], 1, "F" + fldNumStr + ":X1", P1_beam.ControlPoints[0].JawPositions.X1, "", P2_beam.ControlPoints[0].JawPositions.X1, "", "compF" + i.ToString());
                    AddPlanCompItem(clFields[i], 1, "F" + fldNumStr + ":X2", P1_beam.ControlPoints[0].JawPositions.X2, "", P2_beam.ControlPoints[0].JawPositions.X2, "", "compF" + i.ToString());
                    AddPlanCompItem(clFields[i], 1, "F" + fldNumStr + ":Y1", P1_beam.ControlPoints[0].JawPositions.Y1, "", P2_beam.ControlPoints[0].JawPositions.Y1, "", "compF" + i.ToString());
                    AddPlanCompItem(clFields[i], 1, "F" + fldNumStr + ":Y2", P1_beam.ControlPoints[0].JawPositions.Y2, "", P2_beam.ControlPoints[0].JawPositions.Y2, "", "compF" + i.ToString());
                }
            }
        }


        //Utility function for adding items to the comparelists.  Adding all of these at the same time ensures that our index is the same for 
        //all sublists in the PlanCompareLists class object for a given comparison item. 
        public void AddPlanCompItem(PlanCompareLists theList, int dataType, string dataTag, double P1_numData, string P1_stringData, double P2_numData, string P2_stringData, string cbName)
        {
            theList.dataInfo.Add(new CLDataInfo(dataType, dataTag));
            theList.plan1List.Add(new CompareListItem(P1_numData, P1_stringData));
            theList.plan2List.Add(new CompareListItem(P2_numData, P2_stringData));
            theList.resultList.Add(new ResultsItems(cbName));
            theList.count = theList.count + 1;
        }


        //Clear the PlanCompareLists class data, and also clears the PlanCompareResults data.
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



        //A function for comparing plans. Starts by comparing the general info.  Sets marker to red if doesn't match.
        //For field data, starts by assuming that the data for each field matches, and sets the field_okay marker
        //to green.  Then, itereates through and sets the field_okay marker to red if it encounters a discrepancy.
        public void Check_Comparison()
        {
            int compGenCnt = clGeneral.count;
            int compFldCnt = clFields.Count();

            //Iterate through the general info.  Compare either numeric or string data.  Set the results list item to the boolean of whether the
            //two data items are equal.
            //Start from item 1, as item 0 is reserved for the all-fields result, and will be updated below.
            for (int i = 1; i < compGenCnt; i++) {
                if (clGeneral.dataInfo[i].dataType == 1) {
                    clGeneral.resultList[i].result = clGeneral.plan1List[i].numData == clGeneral.plan2List[i].numData;
                    debugTB.AppendText("Plan1 clGenInfo[" + i.ToString() + "] = " + clGeneral.plan1List[i].numData.ToString() + "\r\n");
                    debugTB.AppendText("Plan2 clGenInfo[" + i.ToString() + "] = " + clGeneral.plan2List[i].numData.ToString() + "\r\n");
                }
                if (clGeneral.dataInfo[i].dataType == 2) {
                    clGeneral.resultList[i].result = clGeneral.plan1List[i].stringData == clGeneral.plan2List[i].stringData;
                    debugTB.AppendText("Plan1 clGenInfo[" + i.ToString() + "] = " + clGeneral.plan1List[i].stringData + "\r\n");
                    debugTB.AppendText("Plan2 clGenInfo[" + i.ToString() + "] = " + clGeneral.plan2List[i].stringData + "\r\n");
                }
            }

            if (thePlans[1].Beams.Count() != thePlans[2].Beams.Count()) {
                MessageBox.Show("The two plans have different numbers of fields.  Field comparison cannot be done.");
                return;
            }

            //Iterate through the field info starting with index 1, since index 0 is reserved for representing the entire list.  
            //Compare either numeric or string data.  Set the results list item to the boolean of whether the two data items are equal and the 
            //logical AND of the current state of the field comparison boolean.  This way, if even one paramter compares to 'false', the result
            //for the entire field is set to 'false'.
            bool curAllFieldsBool = clGeneral.resultList[0].result;  //track the cumulative boolean for all fields
            bool curFieldBool = true;                               //track the cumulative boolean for all items for each field
            debugTB.AppendText("Initial All-Fields Bool is" + curAllFieldsBool.ToString() + "\r\n");

            for (int i = 0; i < compFldCnt; i++) {
                int fieldDataItems = clFields[i].count;
                debugTB.AppendText("At start of item[" + i.ToString() + "], All-Fields Bool is" + curAllFieldsBool.ToString() + "\r\n");

                for (int j = 0; j < fieldDataItems; j++) {
                    if (clFields[i].dataInfo[j].dataType == 1) {
                        clFields[i].resultList[j].result = clFields[i].plan1List[j].numData == clFields[i].plan2List[j].numData;
                        debugTB.AppendText("Plan1 clFieldInfo[" + i.ToString() + "][" + j.ToString() + "] = " + clFields[i].plan1List[j].numData.ToString() + "\r\n");
                        debugTB.AppendText("Plan2 clFieldInfo[" + i.ToString() + "][" + j.ToString() + "] = " + clFields[i].plan2List[j].numData.ToString() + "\r\n");
                    }
                    if (clFields[i].dataInfo[j].dataType == 2) {
                        clFields[i].resultList[j].result = clFields[i].plan1List[j].stringData == clFields[i].plan2List[j].stringData;
                        debugTB.AppendText("Plan1 clFieldInfo[" + i.ToString() + "][" + j.ToString() + "] = " + clFields[i].plan1List[j].stringData + "\r\n");
                        debugTB.AppendText("Plan2 clFieldInfo[" + i.ToString() + "][" + j.ToString() + "] = " + clFields[i].plan2List[j].stringData + "\r\n");
                    }
                    curFieldBool = curFieldBool && clFields[i].resultList[j].result;
                    debugTB.AppendText("Bool for item[" + i.ToString() + "][" + j.ToString() + "] = " + curFieldBool.ToString() + "\r\n");
                }
                debugTB.AppendText("At end of item[" + i.ToString() + "], the field Bool is" + curFieldBool.ToString() + "\r\n");
                clFields[i].total_bool = curFieldBool;
                curAllFieldsBool = curAllFieldsBool && curFieldBool;
                //Update the cumulative result to the clGenResults[0] to track the all-fields result.
                debugTB.AppendText("At end of item[" + i.ToString() + "], All-Fields Bool is" + curAllFieldsBool.ToString() + "\r\n");
            }
            clGeneral.resultList[0].result = curAllFieldsBool;
        }
    }

}

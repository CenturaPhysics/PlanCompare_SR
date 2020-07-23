using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PlanCompare_SR_DB;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using static PlanCompare_SR_DB.DataManager;

namespace PlanCompare_SR_DB.Views {
    /// <summary>
    /// Interaction logic for mainWindowContents.xaml
    /// </summary>
    public partial class mainWindowContents : UserControl {


        //public ScriptContext theContext;
        public DataManager theDM;

        //USED for ScriptRunner app only... create placeholders for the current course and current plan index.
        int curCourseIdx;
        int curPlanIdx;

        //Create arrays of lists of TextBlock objects.  These will hold the TextBlock controls for the plan information
        //for each plan.  This gives us a way to update the values of these TextBlocks without needing to repeatedly
        //access the MainWindow children collection.  And, by organizing it by plan, we can create a function to update
        //these that just gets sent the plan num to select the right list of controls.  We create 3 each, and ingnore
        //the index 0 items, so that we can use 1 and 2 as indexes for plans 1 and 2.
        List<TextBlock>[] planGenTBs = new List<TextBlock>[3];
        List<TextBlock>[] planFieldTBs = new List<TextBlock>[3];

        //Create a list of rectangles used to display general plan comparison results. As above, this list is made just to
        //provide an easier way of access these rectangles, rather than always going to the Window child control collection.
        List<Rectangle> rectsGen = new List<Rectangle>();

        //Create a list of rectangles used to display field plan comparison results. As above, this list is made just to
        //provide an easier way of access these rectangles, rather than always going to the Window child control collection.
        List<Rectangle> rectsField = new List<Rectangle>();


        //Empty constructor for MainWindow
        public mainWindowContents(Patient thePatient, PlanSetup thePlanSetup)
        {
            InitializeComponent();

            //Initialize the temporary data objects used by ScriptRunner.  Remove when changed back to PlugIn.
            //REMOVE BELOW FOR PLUGIN APP
            theDM = new DataManager(thePlanSetup);
            theDM.sr_Patient = thePatient;
            theDM.sr_PlanSetup = thePlanSetup;
            //REMOVE ABOVE FOR PLUGIN APP

            //Added just for ScriptRunner.  Sets the mainWindowContents datacontext to be the data manager.
            //For the PlugIn app, the data context is the Script Context, and isn't set here, so remove for PlugIn app.
            //REMOVE FOR PLUGIN APP
            this.DataContext = theDM;
            //REMOVE FOR PLUGIN APP

            //Initialize our lists of plan general info TextBlocks for easy updating of plan info later.
            //Set index 0 to null, so that we can keep the index matching the actual plan number for readability.
            planGenTBs[0] = null;

            planFieldTBs[0] = null;
            planFieldTBs[1] = new List<TextBlock>();
            planFieldTBs[2] = new List<TextBlock>();

            //Code added just for ScriptRunner, to set the initial course and plan
            this.cbCourses1.SelectedIndex = -1;
            this.cbPlans1.SelectedIndex = -1;

            int cnt = theDM.sr_Patient.Courses.Count();
            int foundCourseAt = 0;
            for (int i = 0; i < cnt; i++) {
                if (theDM.sr_Patient.Courses.ElementAt(i).Id == theDM.sr_PlanSetup.Course.Id) {
                    foundCourseAt = i;
                    break;
                }
            }

            cnt = theDM.sr_Patient.Courses.ElementAt(foundCourseAt).PlanSetups.Count();
            int foundPlanAt = 0;
            for (int i = 0; i < cnt; i++) {
                if (theDM.sr_Patient.Courses.ElementAt(foundCourseAt).PlanSetups.ElementAt(i).Id == theDM.sr_PlanSetup.Id) {
                    foundPlanAt = i;
                    break;
                }
            }
            curCourseIdx = foundCourseAt;
            curPlanIdx = foundPlanAt;
        }


        //Sets the current scroll location of the scrollview control to be equal to the scroll bar position.  This allows
        //the scroll bar to drive the scroll viewer.
        public void ScrollBar_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {
            this.svPlan1.ScrollToVerticalOffset(this.sbPlanCompare.Value);
        }

        //This function is used to initialize various controls with the current plan data.  This event is fired last in
        //the chain of events that occur when a window is created.  See: https://wpf.2000things.com/tag/window-events/
        //Used for PlugIn app.  This is the PlugIn version... see UserControl_Loaded below for ScripRunner version of this fucntion.
        //private void Window_Loaded(object sender, RoutedEventArgs e)
        //{
        //    this.sbPlanCompare.Maximum = (this.svPlan1.ExtentHeight - this.svPlan1.ActualHeight);

        //    //Code to initialize the Course and Plan drop-downs... if needed.

        //    //Set the second course combobox to match the first course combobox
        //    this.cbCourses2.SelectedIndex = this.cbCourses1.SelectedIndex;

        //    //Set the plan2 combobox to blank.  Set the plan1 combobox to the first item in it's list.
        //    //NOTE:  seting the SelectedIndex for any of these combobox controls fires their SelectionChanged events.
        //    this.cbPlans2.SelectedIndex = -1;  // would fire cbPlans2_SelectionChanged, but we have left that blank for now.
        //    this.cbPlans1.SelectedIndex = 0;  //immediately fires cbPlans1_SelectionChanged

        //    //Set the initial two rows of the field data grid to height 0.  These rows are the header, and gray separator bar.
        //    this.grid_FieldData.RowDefinitions[0].Height = new GridLength(0);
        //    this.grid_FieldData.RowDefinitions[1].Height = new GridLength(0);

        //    //Set the initial two rows of the field okay grid to height 0.  These rows are both blank.
        //    this.grid_FieldsOkay.RowDefinitions[0].Height = new GridLength(0);
        //    this.grid_FieldsOkay.RowDefinitions[1].Height = new GridLength(0);
        //}


        //This ScriptRunner version of the above function.  Remove for PlugIn app.
        //REMOVE BELOW FOR PLUGIN APP
            private void UserControl_Loaded(object sender, RoutedEventArgs e)
            {
            
                this.sbPlanCompare.Maximum = (this.svPlan1.ExtentHeight - this.svPlan1.ActualHeight);

                //Code to initialize the Course and Plan drop-downs... if needed.

                this.cbCourses1.SelectedIndex = curCourseIdx;
                this.cbPlans1.SelectedIndex = curPlanIdx;

                //Set the second course combobox to match the first course combobox
                this.cbCourses2.SelectedIndex = this.cbCourses1.SelectedIndex;

                //Set the plan2 combobox to blank.  Set the plan1 combobox to the first item in it's list.
                //NOTE:  seting the SelectedIndex for any of these combobox controls fires their SelectionChanged events.
                this.cbPlans2.SelectedIndex = -1;  // would fire cbPlans2_SelectionChanged, but we have left that blank for now.

                //Set the initial two rows of the field data grid to height 0.  These rows are the header, and gray separator bar.
                this.fieldsPanel.Height = 0;

                //Set the initial two rows of the field okay grid to height 0.  These rows are both blank.
                this.field_compOK_header.Height = 0;
                this.field_compOK_panel.Height = 0;

                //theDM.fldOKBrush = new SolidColorBrush(Color.FromRgb(255, 255, 0));
        }
        //REMOVE ABOVE FOR PLUGIN APP


        //Fired when the course selection combobox is changed.  First sets the plan selection combobox to -1, so that it's
        //selection changed events don't end up causing an "object not found error".
        public void cbCourses1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.cbCourses1.SelectedIndex != -1) {
                Course theCourse = (this.cbCourses1.SelectedItem as Course);
                if (theCourse != null) {
                    this.cbPlans1.SelectedIndex = -1;
                    this.cbPlans1.ItemsSource = theCourse.PlanSetups;
                    theDM.theCourses[1] = theCourse;

                    //Also, assume that if the first plan is changed, then the user will want to select a different course 2 and
                    //plan 2, so set both of those comboboxes to -1 as well.  Set the Course 2 combobox to match the Course 1 combobox.
                    this.cbPlans2.SelectedIndex = -1;
                    this.cbCourses2.SelectedIndex = this.cbCourses1.SelectedIndex;
                }
            }
            else {
                //theDM.ClearPlanData(1);
            }
        }


        //Same as above for course 1 combobox.  In this case, don't wipe out all plan 1 data structures.
        public void cbCourses2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (this.cbCourses2.SelectedIndex != -1) {

            //}
            //else {
            //    //theDM.ClearPlanData(2);
            //}

            if ((this.cbCourses2.SelectedItem as Course) != null) {
                this.cbPlans2.SelectedIndex = -1;
                this.cbPlans2.ItemsSource = (this.cbCourses2.SelectedItem as Course).PlanSetups;
                this.cbPlans2.SelectedIndex = -1;
            }
        }


        //Fired when the plan selection combobox is changed.  This can happen when the course selection is changed, and we don't
        //want to respond to that, or we will end up trying to load a plan object that is null.  So, we check to see if this combobox
        //is set to -1.  If so, we ignore the update, and clear plan data if needed (since no plan is selected).
        public void cbPlans1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Only respond if the plan comobox selected index is not -1.
            
            if (this.cbPlans1.SelectedIndex != -1) {
                PlanSetup thePlan = (this.cbPlans1.SelectedItem as PlanSetup);
                if (thePlan != null) {
                    
                    theDM.theCourses[1] = (this.cbCourses1.SelectedItem as Course);

                    //Call the Data Manager function that updates the field data.  
                    theDM.thePlans[1] = new PlanData(thePlan);
                    theDM.NotifyPropertyChanged("thePlans");

                    //theDM.SetPlanData(1, thePlan, this.grid_FieldData, planGenTBs[1], planFieldTBs[1], 0);
                    AddFieldGridRows(this.plan1_fields_panel, theDM.thePlans[1].numOfFields);

                    //Lastly, since we changed plan 1, assume that we will need a different plan 2, so clear all of the old plan 2 data.
                    this.cbPlans2.SelectedIndex = -1;
                    SetComparisonBoxesGray();
                }
                else {
                    MessageBox.Show("Warning: Current PlanSetup for selected 'plan 1' is null.  Cannot use.");
                }
            }
            else {
                //If plan1 selected index = -1, then no plan is selected.  Check to see if there are field data grid rows.
                //If there are, then remove them.  Also, send the ClearPlanData command to the Data Manager.
                if (this.plan1_fields_panel.Children.Count > 0) {
                    SetComparisonBoxesGray();
                    RemoveFieldGridRows(this.plan1_fields_panel);
                }
            }
        }


        //Same as above for plan 1 combobox.
        public void btn_LoadPlan2_Click(object sender, RoutedEventArgs e)
        {
            //Only respond if the plan comobox selected index is not -1.
            if (this.cbPlans2.SelectedIndex != -1) {
                Course selCourse1 = (this.cbCourses1.SelectedItem as Course);
                Course selCourse2 = (this.cbCourses2.SelectedItem as Course);
                PlanSetup curPlan1 = (this.cbPlans1.SelectedItem as PlanSetup);
                PlanSetup curPlan2 = (this.cbPlans2.SelectedItem as PlanSetup); 
                
                if (selCourse1.Id != selCourse2.Id | curPlan1.Id != curPlan2.Id) {
                    if (curPlan2 != null) {

                        theDM.theCourses[2] = (this.cbCourses2.SelectedItem as Course);

                        //Call the Data Manager function that updates the field data.  
                        theDM.thePlans[2] = new PlanData(curPlan2);
                        theDM.NotifyPropertyChanged("thePlans");

                        AddFieldGridRows(this.plan2_fields_panel, theDM.thePlans[2].numOfFields);
                        
                        theDM.SetCompareLists();
                        theDM.Check_General_Comparisons();

                        if(theDM.Check_Field_Comparisons() == "OK"){
                            UpdateComparisonBoxes();
                        }
                        else {
                            SetComparisonBoxesGray();
                        }
                        
                    }
                    else {
                        MessageBox.Show("Warning:  Current PlanSetup for selected 'plan 2' is null.  Cannot use.");
                    }
                }
                else {
                    SetComparisonBoxesGray();
                }
            }
        }


        public void AddFieldGridRows(StackPanel theSP, int numOfRows)
        {
            //Create new RowDefinition and then, for each field in the plan, add a row to both the field_data grid
            //and the field_okay grid.  For the field_okay grid, add a rectangle for the comparison marker.

            //Remove all filed rows to start from a clean slate.
            RemoveFieldGridRows(theSP);

            for (int i = 0; i < numOfRows; i++) {
                MakeNewFieldPanel(theSP);
            }

            int numOfFlds = Math.Max(this.plan1_fields_panel.Children.Count, this.plan2_fields_panel.Children.Count);
            int numOfOKRows = this.field_compOK_panel.Children.Count;
            if (numOfOKRows < numOfRows) {
                for (int i = 0; i < (numOfFlds - numOfOKRows); i++) {
                    //Create new rectangles for the field_okay grid.  Set the color to Green.
                    Rectangle newRect = new Rectangle();
                    newRect.Height = 16;
                    newRect.Width = 20;
                    newRect.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                    newRect.Margin = new Thickness(0, 2, 0, 2);
                    newRect.Name = "compF" + i.ToString();
                    this.field_compOK_panel.Children.Add(newRect);  // <--  Create new funtion to make a field OK checkbox panel

                    //Add the rectangle to the rectsField list.
                    this.rectsField.Add(newRect);
                }
            }
        }


        //Utility function 
        public StackPanel MakeNewFieldPanel(StackPanel parentPanel)
        {
            StackPanel newSP = new StackPanel();
            newSP.Orientation = Orientation.Horizontal;
            newSP.Width = 447;
            newSP.Height = 20;
            parentPanel.Children.Add(newSP);            
            
            int curFldNum = parentPanel.Children.Count;
            if(parentPanel.Uid == "" | parentPanel.Uid == null) {
                MessageBox.Show("Expect plan number in " + nameof(parentPanel) + "'s UID property.  Instead, UID is blank or null.");
                System.Windows.Application.Current.Shutdown();
                return newSP;
            }
            else {
                int curPlanNum = Convert.ToInt32(parentPanel.Uid);
                FieldData curField = theDM.thePlans[curPlanNum].fields[curFldNum];

                AddFieldTextBlockToPanel(newSP, curPlanNum, curFldNum, 70, nameof(curField.fieldId), curField);
                AddFieldTextBlockToPanel(newSP, curPlanNum, curFldNum, 50, nameof(curField.gantryAngle), curField);
                AddFieldTextBlockToPanel(newSP, curPlanNum, curFldNum, 50, nameof(curField.collAngle), curField);
                AddFieldTextBlockToPanel(newSP, curPlanNum, curFldNum, 50, nameof(curField.tableAngle), curField);
                AddFieldTextBlockToPanel(newSP, curPlanNum, curFldNum, 40, nameof(curField.X1), curField);
                AddFieldTextBlockToPanel(newSP, curPlanNum, curFldNum, 40, nameof(curField.X2), curField);
                AddFieldTextBlockToPanel(newSP, curPlanNum, curFldNum, 40, nameof(curField.Y1), curField);
                AddFieldTextBlockToPanel(newSP, curPlanNum, curFldNum, 40, nameof(curField.Y2), curField);
                AddFieldTextBlockToPanel(newSP, curPlanNum, curFldNum, 50, nameof(curField.MUs), curField);
                return newSP;
            }
        }


        public void AddFieldTextBlockToPanel(StackPanel theSP, int planNum, int fldNum, int width, string idText, object curFieldProp)
        {
            TextBlock newTb = new TextBlock();
            newTb.Width = width;
            newTb.Name = "p" + planNum + "_f" + fldNum + "_" + idText;
            newTb.TextAlignment = TextAlignment.Center;
            newTb.FontSize = 12;
            theSP.Children.Add(newTb);

            Binding b = new Binding(idText);
            b.Source = curFieldProp;
            b.Mode = BindingMode.OneWay;
            b.StringFormat = "{0:N1}";
            newTb.SetBinding(TextBlock.TextProperty, b);
        }


        public void RemoveFieldGridRows(StackPanel theSP)
        {
            //First, get the number or rows in the field data grid
            int curNumOfRows = theSP.Children.Count;
            if (curNumOfRows > 0) {
                theSP.Children.Clear(); 
            }

            //Repeat for the field okay grid...
            curNumOfRows = this.field_compOK_panel.Children.Count;
            if (curNumOfRows > 0) {
                this.field_compOK_panel.Children.Clear(); 
            }

            //Clear the rectsField list
            rectsField.Clear();

        }


        //Sets all the plan comparison result boxes to be green
        public void SetComparisonBoxesGray()
        {
            var rects = this.general_compOK_panel.Children.OfType<Rectangle>();

            foreach(Rectangle rect in rects) {
                rect.Fill = new SolidColorBrush(Color.FromRgb(128, 128, 128));
            }

            foreach (Rectangle rect in this.field_compOK_panel.Children) {
                rect.Fill = new SolidColorBrush(Color.FromRgb(128, 128, 128));
            }
        }


        //Sets the color of the general and field comparison results boxes... currently just a green or red rectangle.
        //For the general info markers, some of these markers consolidate more than 1 data point (such as the fractionation results) 
        //where each data point is checked separately.  So, before we set the result marker, we first check to see if it's already red, 
        //and only allow it to be set to green if it's not already red.
        public void UpdateComparisonBoxes()
        {
            //int cntGen = theDM.planCompareResults.clGenResults.Count();
            int cntGen = theDM.clGeneral.count;
            int cntFld = theDM.clFields.Count();

            //iterate through the general comparison list, setting the result boxes to red or green appropriately
            for (int i = 0; i < cntGen; i++) {
                Rectangle theRect = (Rectangle)GetControlByName(this.general_compOK_panel, theDM.clGeneral.resultList[i].cbName);

                if (theDM.clGeneral.resultList[i].result == true) {
                    theRect.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                }
                else {
                    theRect.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                }
            }

            //iterate through the list of field comparison lists, setting the result boxes to red or green appropriately
            for (int i = 0; i < cntFld; i++) {
                int cntFldItems = theDM.clFields[i].count;
                for (int j = 0; j < cntFldItems; j++) {
                    Rectangle theRect = (Rectangle)GetControlByName(this.field_compOK_panel, theDM.clFields[i].resultList[j].cbName);

                    if (theDM.clFields[i].total_bool == true) {
                        theRect.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                    }
                    else {
                        theRect.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0)); 
                    }
                }  
            }
            this.UpdateLayout();
        }


        //A utility function to return the child control contained in the panel (passed as parameter 1) given the child control's name (parameter 2).
        //Returns null if not found.
        public FrameworkElement GetControlByName(Panel aContainer, string objName)
        {
            int i = 0;
            foreach (FrameworkElement winControl in aContainer.Children) {
                if (winControl.Name == objName) {
                    return winControl;
                }
            }
            return null;
        }


        //An interface function for expanding the field data list.  Simply changes the grid heights for the field_data and
        //field_okay grids to their nominal values.
        public void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            this.fieldsPanel.Height = Double.NaN;
            this.field_compOK_header.Height = 24;
            this.field_compOK_panel.Height = Double.NaN;
        }


        //An interface function for collapsing the field data list.  Simply changes the grid heights for the field_data and
        //field_okay grids to zero (0).  Note, the grid rows and all their controls are still there, you just can't see them.
        public void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            this.fieldsPanel.Height = 0;
            this.field_compOK_header.Height = 0;
            this.field_compOK_panel.Height = 0;
        }


        private void btn_percent_up_Click(object sender, RoutedEventArgs e)
        {
            theDM.passingThreshold = theDM.passingThreshold + 1;
        }


        private void btn_percent_down_Click(object sender, RoutedEventArgs e)
        {
            theDM.passingThreshold = theDM.passingThreshold - 1;
        }


        private void tbx_passThreshold_TextChanged(object sender, TextChangedEventArgs e)
        {
            //theDM.passingThreshold = Convert.ToDouble(this.tbx_passThreshold.Text);
        }

        private void btn_plus_one_Click(object sender, RoutedEventArgs e)
        {
            theDM.passingThreshold = theDM.passingThreshold + 1.0;
            theDM.UpdatePlanDoseThresholds();
            UpdateComparisonBoxes();
        }

        private void btn_minus_one_Click(object sender, RoutedEventArgs e)
        {
            theDM.passingThreshold = theDM.passingThreshold - 1.0;
            theDM.UpdatePlanDoseThresholds();
            UpdateComparisonBoxes();
        }

        private void btn_plus_one_tenth_Click(object sender, RoutedEventArgs e)
        {
            theDM.passingThreshold = theDM.passingThreshold + 0.1;
            theDM.UpdatePlanDoseThresholds();
            UpdateComparisonBoxes();
        }

        private void btn_minus_one_tenth_Click(object sender, RoutedEventArgs e)
        {
            theDM.passingThreshold = theDM.passingThreshold - 0.1;
            theDM.UpdatePlanDoseThresholds();
            UpdateComparisonBoxes();
        }

        private void tbx_passThreshold_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            double newPT = Convert.ToDouble(this.tbx_passThreshold.Text);
            theDM.passingThreshold = newPT;
            theDM.UpdatePlanDoseThresholds();
            UpdateComparisonBoxes();
        }

    }
}

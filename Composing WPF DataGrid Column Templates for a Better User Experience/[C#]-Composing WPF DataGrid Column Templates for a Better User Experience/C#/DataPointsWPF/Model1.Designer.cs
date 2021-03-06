//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Data.EntityClient;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Runtime.Serialization;

[assembly: EdmSchemaAttribute()]

namespace DataPointsWPF
{
    #region Contexts
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    public partial class DataPointsSchedulingEntities : ObjectContext
    {
        #region Constructors
    
        /// <summary>
        /// Initializes a new DataPointsSchedulingEntities object using the connection string found in the 'DataPointsSchedulingEntities' section of the application configuration file.
        /// </summary>
        public DataPointsSchedulingEntities() : base("name=DataPointsSchedulingEntities", "DataPointsSchedulingEntities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new DataPointsSchedulingEntities object.
        /// </summary>
        public DataPointsSchedulingEntities(string connectionString) : base(connectionString, "DataPointsSchedulingEntities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new DataPointsSchedulingEntities object.
        /// </summary>
        public DataPointsSchedulingEntities(EntityConnection connection) : base(connection, "DataPointsSchedulingEntities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        #endregion
    
        #region Partial Methods
    
        partial void OnContextCreated();
    
        #endregion
    
        #region ObjectSet Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<ScheduleItem> ScheduleItems
        {
            get
            {
                if ((_ScheduleItems == null))
                {
                    _ScheduleItems = base.CreateObjectSet<ScheduleItem>("ScheduleItems");
                }
                return _ScheduleItems;
            }
        }
        private ObjectSet<ScheduleItem> _ScheduleItems;

        #endregion
        #region AddTo Methods
    
        /// <summary>
        /// Deprecated Method for adding a new object to the ScheduleItems EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToScheduleItems(ScheduleItem scheduleItem)
        {
            base.AddObject("ScheduleItems", scheduleItem);
        }

        #endregion
    }
    

    #endregion
    
    #region Entities
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName="DataPointsSchedulingModel", Name="ScheduleItem")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class ScheduleItem : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new ScheduleItem object.
        /// </summary>
        /// <param name="scheduleId">Initial value of the ScheduleId property.</param>
        /// <param name="dateScheduled">Initial value of the DateScheduled property.</param>
        /// <param name="resultsStatus">Initial value of the ResultsStatus property.</param>
        /// <param name="repeatAnnually">Initial value of the RepeatAnnually property.</param>
        public static ScheduleItem CreateScheduleItem(global::System.Int32 scheduleId, global::System.DateTime dateScheduled, global::System.String resultsStatus, global::System.Boolean repeatAnnually)
        {
            ScheduleItem scheduleItem = new ScheduleItem();
            scheduleItem.ScheduleId = scheduleId;
            scheduleItem.DateScheduled = dateScheduled;
            scheduleItem.ResultsStatus = resultsStatus;
            scheduleItem.RepeatAnnually = repeatAnnually;
            return scheduleItem;
        }

        #endregion
        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 ScheduleId
        {
            get
            {
                return _ScheduleId;
            }
            set
            {
                if (_ScheduleId != value)
                {
                    OnScheduleIdChanging(value);
                    ReportPropertyChanging("ScheduleId");
                    _ScheduleId = StructuralObject.SetValidValue(value);
                    ReportPropertyChanged("ScheduleId");
                    OnScheduleIdChanged();
                }
            }
        }
        private global::System.Int32 _ScheduleId;
        partial void OnScheduleIdChanging(global::System.Int32 value);
        partial void OnScheduleIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.DateTime DateScheduled
        {
            get
            {
                return _DateScheduled;
            }
            set
            {
                OnDateScheduledChanging(value);
                ReportPropertyChanging("DateScheduled");
                _DateScheduled = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("DateScheduled");
                OnDateScheduledChanged();
            }
        }
        private global::System.DateTime _DateScheduled;
        partial void OnDateScheduledChanging(global::System.DateTime value);
        partial void OnDateScheduledChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String Task
        {
            get
            {
                return _Task;
            }
            set
            {
                OnTaskChanging(value);
                ReportPropertyChanging("Task");
                _Task = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Task");
                OnTaskChanged();
            }
        }
        private global::System.String _Task;
        partial void OnTaskChanging(global::System.String value);
        partial void OnTaskChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.DateTime> DatePerformed
        {
            get
            {
                return _DatePerformed;
            }
            set
            {
                OnDatePerformedChanging(value);
                ReportPropertyChanging("DatePerformed");
                _DatePerformed = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("DatePerformed");
                OnDatePerformedChanged();
            }
        }
        private Nullable<global::System.DateTime> _DatePerformed;
        partial void OnDatePerformedChanging(Nullable<global::System.DateTime> value);
        partial void OnDatePerformedChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String ResultsStatus
        {
            get
            {
                return _ResultsStatus;
            }
            set
            {
                OnResultsStatusChanging(value);
                ReportPropertyChanging("ResultsStatus");
                _ResultsStatus = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("ResultsStatus");
                OnResultsStatusChanged();
            }
        }
        private global::System.String _ResultsStatus;
        partial void OnResultsStatusChanging(global::System.String value);
        partial void OnResultsStatusChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Boolean RepeatAnnually
        {
            get
            {
                return _RepeatAnnually;
            }
            set
            {
                OnRepeatAnnuallyChanging(value);
                ReportPropertyChanging("RepeatAnnually");
                _RepeatAnnually = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("RepeatAnnually");
                OnRepeatAnnuallyChanged();
            }
        }
        private global::System.Boolean _RepeatAnnually;
        partial void OnRepeatAnnuallyChanging(global::System.Boolean value);
        partial void OnRepeatAnnuallyChanged();

        #endregion
    
    }

    #endregion
    
}

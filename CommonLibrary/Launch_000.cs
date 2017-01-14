﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
//using Autodesk.Windows;
//ing System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
//using System.IO;
using System.Reflection;

//using App = Autodesk.AutoCAD.ApplicationServices.Application;
//using AppService = Autodesk.AutoCAD.ApplicationServices;
//using RunTime = Autodesk.AutoCAD.Runtime;

namespace Autodesk.Cad.Crushner.Common
{
    public abstract class Launch_000 : IExtensionApplication
    {
        /// <summary>
        /// Наименование шаблона для создания нового документа(чертежа)
        /// </summary>
        private string s_acTemplateDocumentName;
        /// <summary>
        /// Наименование нового документа
        /// </summary>
        private string _acCurrentDocumentName;
        /// <summary>
        /// Признак создания нового документа
        /// </summary>
        protected bool DocumentCreated { get { return _acCurrentDocumentName.Equals(string.Empty) == false; } }
        /// <summary>
        /// Счетчик выполнения команды "Масштабирование"
        /// </summary>
        private int iScallingCounter;

        #region Обязательные методы плюгина
        /// <summary>
        /// Функция инициализации (выполняется при загрузке плагина)
        /// </summary>
        public virtual void Initialize()
        {
            Logging.DebugCaller(MethodBase.GetCurrentMethod());

            s_acTemplateDocumentName = @"acad3d.dwt";

            _acCurrentDocumentName = string.Empty;

            iScallingCounter = -1;

            //AppDomain.CurrentDomain.UnhandledException += ProgramBase.CurrentDomain_UnhandledException;

            // для создания нового 3д-окна
            //Autodesk.AutoCAD.ApplicationServices.Application.Idle += new EventHandler(Application_Idle);
            closeAndDiscard();

            acadNewDoc();
        }

        protected virtual void reinitialize()
        {
            Logging.DebugCaller(MethodBase.GetCurrentMethod());

            Database dbCurrent = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
            DocumentCollection acDocMgr = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;
            Document acDoc = acDocMgr.GetDocument(dbCurrent);

            _acCurrentDocumentName = acDoc.Name;

            iScallingCounter = -1;

            //AppDomain.CurrentDomain.UnhandledException += ProgramBase.CurrentDomain_UnhandledException;
        }
        /// <summary>
        /// Функция, выполняемая при выгрузке плагина
        /// </summary>
        public void Terminate()
        {
            Logging.DebugCaller(MethodBase.GetCurrentMethod());
        }
        #endregion

        #region Обработчики событий
        ///// <summary>
        ///// Обработчик события - приложение ожидает обработки команд (свободно)
        ///// </summary>
        ///// <param name="sender">Объект, инициировавший событие</param>
        ///// <param name="e">Аргумент события</param>
        //protected virtual void Application_Idle(object sender, EventArgs e)
        //{
        //    Autodesk.AutoCAD.ApplicationServices.Application.Idle -= Application_Idle;
        //}
        ///// <summary>
        ///// Обработчик события - создание нового документа завершено
        ///// </summary>
        ///// <param name="sender">Объект, инициировавший событие</param>
        ///// <param name="e">Аргумент события</param>
        //protected virtual void AcDocMgr_DocumentCreated(object sender, DocumentCollectionEventArgs e)
        //{
        //    Database dbCurrent = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
        //    DocumentCollection acDocMgr = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;
        //    Document acDoc = acDocMgr.GetDocument(dbCurrent);
        //    _acCurrentDocumentName = acDoc.Name;

        //    acDoc.Editor.WriteMessage(string.Format(@"{0}Текущий документ: создан с именем {1}.", Environment.NewLine, _acCurrentDocumentName));
        //}
        #endregion

        #region Методы плагина для выполнения команд
        /// <summary>
        /// Метод плагина для выполнения команды - масштабировать примитивы
        /// </summary>
        [CommandMethod("LAUNCH-000-ZOOM")]
        public void EntityZoom()
        {
            entityScaling();
        }
        #endregion

        #region Управление (в т.ч. добавление) примитивов
        protected float Scale { get { return (iScallingCounter % 2) == 1 ? 2 : 0.5F; } }
        /// <summary>
        /// Масштабировать примитивы
        /// </summary>
        private void entityScaling()
        {
            iScallingCounter++;

            // получаем текущий документ и его БД
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            // начинаем транзакцию
            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                try {
                    // получаем ссылку на пространство модели (ModelSpace)
                    // открываем таблицу блоков документа
                    BlockTable acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

                    // открываем пространство модели (Model Space) - оно является одной из записей в таблице блоков документа
                    BlockTableRecord ms = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    // "пробегаем" по всем объектам в пространстве модели
                    foreach (ObjectId id in ms) {
                        // приводим каждый из них к типу Entity
                        Entity entity = (Entity)tr.GetObject(id, OpenMode.ForWrite);

                        PropertyInfo propInfo = entity.GetType().GetProperty(@"Center");
                        if ((!(propInfo == null))
                            && (propInfo.CanRead == true)) {
                            entity.TransformBy(Matrix3d.Scaling(Scale, (Point3d)propInfo.GetValue(entity, null)));

                            // выводим в консоль слой (entity.Layer), тип (entity.GetType().ToString()) и цвет (entity.Color) каждого объекта
                            doc.Editor.WriteMessage(string.Format("\nСлой:{0}; тип:{1}; цвет: [{2},{3},{4}]\n",
                                entity.Layer, entity.GetType().ToString(), entity.Color.Red.ToString(), entity.Color.Green.ToString(), entity.Color.Blue.ToString()));
                        } else
                            // выводим в консоль слой (entity.Layer), тип (entity.GetType().ToString()) и цвет (entity.Color) каждого объекта
                            doc.Editor.WriteMessage(string.Format("\nСлой:{0}; тип:{1}; выполнение [LAUNCH-000-ZOOM] в настоящее время не поддерживается\n",
                                entity.Layer, entity.GetType().ToString()));
                    }

                    tr.Commit();
                } catch (System.Exception e) {
                    Logging.ExceptionCaller(MethodBase.GetCurrentMethod(), e);

                    Logging.AcEditorWriteException(e, @"LAUNCH-000-ZOOM");
                }
            }
        }

        private void acadNewDoc()
        {
            DocumentCollection acDocMgr = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;

            Logging.DebugCaller(MethodBase.GetCurrentMethod());

            //Добавить обработчик события - документ создан
            //acDocMgr.DocumentCreated += new DocumentCollectionEventHandler(AcDocMgr_DocumentCreated);
            //Добавить новый документ на основе шаблона
            acDocMgr.Add(s_acTemplateDocumentName);
        }

        private void closeAndDiscard()
        {
            Database dbCurrent = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
            DocumentCollection acDocMgr = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;
            Document acDoc = acDocMgr.GetDocument(dbCurrent);

            //acDoc.CloseAndDiscard();

            //acDoc.CommandEnded += new CommandEventHandler (AcDoc_CommandEnded);
            acDoc.SendStringToExecute(@"_close ", true, false, false);

            //object acadObject = Application.AcadApplication;
            //object ActiveDocument = acadObject.GetType().InvokeMember("ActiveDocument"
            //    , BindingFlags.GetProperty
            //    , null
            //    , acadObject
            //    , null
            //);

            //ActiveDocument.GetType().InvokeMember("close"
            //    , BindingFlags.InvokeMethod
            //    , null
            //    , ActiveDocument
            //    , new object[] { false, string.Empty }
            //);
        }

        private void AcDoc_CommandEnded(object sender, CommandEventArgs e)
        {
            if (!(e.GlobalCommandName.IndexOf(@"close", StringComparison.CurrentCultureIgnoreCase) < 0))
                acadNewDoc();
            else
                ;
        }
        #endregion

        #region Общие методы по работе с примитивами
        /// <summary>
        /// Очистить чертеж от всех примитивов
        /// </summary>
        protected void clear()
        {
            Database db = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;

            // начинаем транзакцию
            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                // получаем ссылку на пространство модели (ModelSpace)
                // открываем таблицу блоков документа
                BlockTable acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

                // открываем пространство модели (Model Space) - оно является одной из записей в таблице блоков документа
                BlockTableRecord ms = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // "пробегаем" по всем объектам в пространстве модели
                foreach (ObjectId id in ms) {
                    // приводим каждый из них к типу Entity
                    Entity entity = (Entity)tr.GetObject(id, OpenMode.ForWrite);

                    // выводим в консоль слой (entity.Layer), тип (entity.GetType().ToString()) и цвет (entity.Color) каждого объекта
                    Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(string.Format("\n!!!Для удаления - слой:{0}; тип:{1}; цвет: [{2},{3},{4}]\n",
                        entity.Layer, entity.GetType().ToString(), entity.Color.Red.ToString(), entity.Color.Green.ToString(), entity.Color.Blue.ToString()));

                    entity.Erase();
                }

                tr.Commit();
            }
        }

        protected void export(string nameFileSettings, MSExcel.FORMAT format)
        {
            // получаем текущий документ и его БД
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            // начинаем транзакцию
            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                // получаем ссылку на пространство модели (ModelSpace)
                // открываем таблицу блоков документа
                BlockTable acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

                // открываем пространство модели (Model Space) - оно является одной из записей в таблице блоков документа
                BlockTableRecord ms = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // "пробегаем" по всем объектам в пространстве модели
                foreach (ObjectId id in ms) {
                    // приводим каждый из них к типу Entity
                    Entity entity = (Entity)tr.GetObject(id, OpenMode.ForRead);

                    MSExcel.AddToExport(entity);

                    // выводим в консоль слой (entity.Layer), тип (entity.GetType().ToString()) и цвет (entity.Color) каждого объекта
                    doc.Editor.WriteMessage(string.Format("\nПодготовлен для экспорта: слой:{0}; тип:{1}; цвет: [{2},{3},{4}]\n",
                        entity.Layer, entity.GetType().ToString(), entity.Color.Red.ToString(), entity.Color.Green.ToString(), entity.Color.Blue.ToString()));
                }

                tr.Commit();
            }

            MSExcel.Export(nameFileSettings, format);
        }

        /// <summary>
        /// Добавить примитив из коллекции
        /// </summary>
        /// <param name="key">Ключ примитива из коллекции</param>
        protected void entityAdd(KEY_ENTITY key)
        {
            Database dbCurrent = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
            BlockTableRecord btrCurrSpace;
            ObjectId oidEntity;

            using (Transaction trAdding = dbCurrent.TransactionManager.StartTransaction())
            {
                btrCurrSpace = trAdding.GetObject(dbCurrent.CurrentSpaceId
                    , OpenMode.ForWrite) as BlockTableRecord;

                oidEntity = btrCurrSpace.AppendEntity(MSExcel.s_dictEntity[key]);
                trAdding.AddNewlyCreatedDBObject(MSExcel.s_dictEntity[key], true);

                trAdding.Commit();
            }

            DocumentCollection acDocMgr = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;
            Document acDoc = acDocMgr.GetDocument(dbCurrent);
            acDoc.Editor.WriteMessage(string.Format(@"{0}Добавлен примитив {1}...", Environment.NewLine, MSExcel.s_dictEntity[key].GetType().Name));
        }

        protected void flash()
        {
            if (MSExcel.s_dictEntity.Count > 0)
                foreach (KEY_ENTITY key in MSExcel.s_dictEntity.Keys)
                    entityAdd(key);
            else
            {
                Database dbCurrent = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
                DocumentCollection acDocMgr = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;
                Document acDoc = acDocMgr.GetDocument(dbCurrent);
                acDoc.Editor.WriteMessage(string.Format(@"{0}Команда ИМПОРТ не выполнена либо нет объектов...", Environment.NewLine));
            }
        }
        #endregion
    }
}
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
using Autodesk.AutoCAD.Interop.Common;

//using App = Autodesk.AutoCAD.ApplicationServices.Application;
//using AppService = Autodesk.AutoCAD.ApplicationServices;
//using RunTime = Autodesk.AutoCAD.Runtime;

namespace Autodesk.Cad.Crushner.Assignment
{
    public abstract class Command : IExtensionApplication
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
            Logging.AcEditorDebugCaller(MethodBase.GetCurrentMethod());

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
            Logging.AcEditorDebugCaller(MethodBase.GetCurrentMethod());

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
            Logging.AcEditorDebugCaller(MethodBase.GetCurrentMethod());
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
        [CommandMethod("CRU-ZOOM")]
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
                        // приводим каждый из них к типу object
                        object entity = (object)tr.GetObject(id, OpenMode.ForWrite);

                        //PropertyInfo propInfo = entity.GetType().GetProperty(@"Center");
                        //if ((!(propInfo == null))
                        //    && (propInfo.CanRead == true)) {
                            (entity is Entity ? entity as Entity : entity is Solid3d ? entity as Solid3d : null).TransformBy(
                                Matrix3d.Scaling(
                                    Scale
                                    , Point3d.Origin // (Point3d)propInfo.GetValue(entity, null)
                                ));

                            // выводим в консоль слой (entity.Layer), тип (entity.GetType().ToString()) и цвет (entity.Color) каждого объекта
                            doc.Editor.WriteMessage(string.Format("\nСлой:{0}; тип:{1}; цвет: [{2},{3},{4}]\n",
                                (entity is Entity ? entity as Entity : entity is Solid3d ? entity as Solid3d : null).Layer
                                , entity.GetType().ToString()
                                , (entity is Entity ? entity as Entity : entity is Solid3d ? entity as Solid3d : null).Color.Red.ToString()
                                , (entity is Entity ? entity as Entity : entity is Solid3d ? entity as Solid3d : null).Color.Green.ToString()
                                , (entity is Entity ? entity as Entity : entity is Solid3d ? entity as Solid3d : null).Color.Blue.ToString())
                            );
                        //} else
                        //    // выводим в консоль слой (entity.Layer), тип (entity.GetType().ToString()) и цвет (entity.Color) каждого объекта
                        //    doc.Editor.WriteMessage(string.Format("\nСлой:{0}; тип:{1}; выполнение [CRU-ZOOM] в настоящее время не поддерживается\n",
                        //        (entity is Entity ? entity as Entity : entity is Solid3d ? entity as Solid3d : null).Layer
                        //        , entity.GetType().ToString()));
                    }

                    tr.Commit();
                } catch (System.Exception e) {
                    Logging.ExceptionCaller(MethodBase.GetCurrentMethod(), e);

                    Logging.AcEditorWriteException(e, @"CRU-ZOOM");
                }
            }
        }

        private void acadNewDoc()
        {
            DocumentCollection acDocMgr = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;

            Logging.AcEditorDebugCaller(MethodBase.GetCurrentMethod());

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
            List<string> listBlockNameToErase = new List<string>();

            // начинаем транзакцию
            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                // получаем ссылку на пространство модели (ModelSpace)
                // открываем таблицу блоков документа
                BlockTable acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

                // открываем пространство модели (Model Space) - оно является одной из записей в таблице блоков документа
                BlockTableRecord ms = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // "пробегаем" по всем объектам в пространстве модели
                foreach (ObjectId id in ms) {
                    // приводим каждый из них к типу object
                    object entity = (object)tr.GetObject(id, OpenMode.ForWrite);

                    if (!(entity is BlockReference)) {
                        // выводим в консоль слой (entity.Layer), тип (entity.GetType().ToString()) и имя блока для объекта
                        Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(
                            string.Format("\n!!!Для удаления - слой:{0}; тип:{1}; имя: {2}\n",
                                (entity is Entity ? entity as Entity : entity is Solid3d ? entity as Solid3d : null).Layer
                                , (entity is Entity ? entity as Entity : entity is Solid3d ? entity as Solid3d : null).GetType().ToString()
                                , (entity is Entity ? entity as Entity : entity is Solid3d ? entity as Solid3d : null).BlockName)
                        );

                        (entity is Entity ? entity as Entity : entity is Solid3d ? entity as Solid3d : null)?.Erase();
                    } else
                        if (listBlockNameToErase.IndexOf((entity as BlockReference).Name) < 0)
                            listBlockNameToErase.Add((entity as BlockReference).Name);
                        else
                            ;
                }

                tr.Commit();
            }

            listBlockNameToErase.ForEach(blockName => { EraseBlock(blockName); });
        }

        #region ??? Тест - Удаление блока
        public void EraseBlock(string blockName)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            AutoCAD.EditorInput.Editor ed = doc.Editor;

            try {
                var blockId = getBlockId(db, blockName);
                if (blockId.IsNull)
                    throw new System.Exception(string.Format("\n Block not found: {0}", blockName));

                if (!eraseBlockReferences(blockId))
                    throw new System.Exception(string.Format("\n Failed to erase Block References for: {0}", blockName));

                if (!eraseBlockDefinition(blockId))
                    throw new System.Exception(string.Format("\n Failed to erase Block Definition: {0}", blockName));

                ed.WriteMessage("\n Block full erased: {0}", blockName);
            } catch (System.Exception ex) {
                ed.WriteMessage(ex.Message);
            }
        }

        protected static ObjectId getBlockId(Database db, string blockName)
        {

            ObjectId blkId = ObjectId.Null;

            if (db == null)
                return ObjectId.Null;

            if (string.IsNullOrWhiteSpace(blockName))
                return ObjectId.Null;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                if (bt.Has(blockName))
                    blkId = bt[blockName];
                tr.Commit();
            }
            return blkId;
        }

        protected static bool eraseBlockReferences(ObjectId blockId)
        {
            bool bRes = false;

            if (blockId.IsNull)
                return false;

            Database db = blockId.Database;
            if (db == null)
                return false;

            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                BlockTableRecord blk = (BlockTableRecord)tr.GetObject(blockId, OpenMode.ForRead);
                var blkRefs = blk.GetBlockReferenceIds(true, true);
                if (blkRefs != null && blkRefs.Count > 0) {
                    foreach (ObjectId blkRefId in blkRefs) {
                        BlockReference blockRef = (BlockReference)tr.GetObject(blkRefId, OpenMode.ForWrite);
                        blockRef.Erase();
                    }

                    bRes = true;
                } else
                    ;

                tr.Commit();
            }

            return bRes;
        }

        protected static bool eraseBlockDefinition(ObjectId blockId)
        {
            bool blkIsErased = false;

            if (blockId.IsNull)
                return false;

            Database db = blockId.Database;
            if (db == null)
                return false;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {

                BlockTableRecord blk = (BlockTableRecord)tr.GetObject(blockId, OpenMode.ForRead);
                var blkRefs = blk.GetBlockReferenceIds(true, true);
                if (blkRefs == null || blkRefs.Count == 0)
                {
                    blk.UpgradeOpen();
                    blk.Erase();
                    blkIsErased = true;
                }
                tr.Commit();
            }
            return blkIsErased;
        }
        #endregion

        #region ??? Export - не реализован
        //protected void export(string nameFileSettings, MSExcel.FORMAT format)
        //{
        //    // получаем текущий документ и его БД
        //    Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
        //    Database db = doc.Database;

        //    // начинаем транзакцию
        //    using (Transaction tr = db.TransactionManager.StartTransaction()) {
        //        // получаем ссылку на пространство модели (ModelSpace)
        //        // открываем таблицу блоков документа
        //        BlockTable acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

        //        // открываем пространство модели (Model Space) - оно является одной из записей в таблице блоков документа
        //        BlockTableRecord ms = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

        //        // "пробегаем" по всем объектам в пространстве модели
        //        foreach (ObjectId id in ms) {
        //            // приводим каждый из них к типу object
        //            object entity = (object)tr.GetObject(id, OpenMode.ForRead);

        //            MSExcel.AddToExport(entity);

        //            // выводим в консоль слой (entity.Layer), тип (entity.GetType().ToString()) и цвет (entity.Color) каждого объекта
        //            doc.Editor.WriteMessage(
        //                string.Format("\nПодготовлен для экспорта: слой:{0}; тип:{1}; цвет: [{2},{3},{4}]\n",
        //                    (entity is Entity ? entity as Entity : entity is Solid3d ? entity as Solid3d : null).Layer
        //                    , (entity is Entity ? entity as Entity : entity is Solid3d ? entity as Solid3d : null).GetType().ToString()
        //                    , (entity is Entity ? entity as Entity : entity is Solid3d ? entity as Solid3d : null).Color.Red.ToString()
        //                    , (entity is Entity ? entity as Entity : entity is Solid3d ? entity as Solid3d : null).Color.Green.ToString()
        //                    , (entity is Entity ? entity as Entity : entity is Solid3d ? entity as Solid3d : null).Color.Blue.ToString())
        //            );
        //        }

        //        tr.Commit();
        //    }

        //    MSExcel.Export(nameFileSettings, format);
        //}
        #endregion

        /// <summary>
        /// Добавить примитивы в блок из коллекции
        /// </summary>
        /// <param name="blockName">Наименования блока - владельца прмитива</param>
        protected void entitiesAdd(string blockName)
        {
            Database dbCurrent = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
            BlockTable btCurrSpace;
            BlockTableRecord btrCurrent;
            ObjectId oidBrecord
                , oidEntity;
            // для трансформации в исходное положение и извлесения значений параметров при создании примитива
            //Entity entityTransformCopy;

            string message = string.Format(@"Добавление примитивов для блока {0}..."
                , blockName);

            try {
                using (Transaction trCurrent = dbCurrent.TransactionManager.StartTransaction()) {
                    // открываем таблицу блоков на запись
                    btCurrSpace = trCurrent.GetObject(dbCurrent.BlockTableId, OpenMode.ForWrite) as BlockTable;

                    if (blockName.Equals(string.Empty) == false) {
                    // найти блок; при отсутствии создать
                        if (btCurrSpace.Has(blockName) == true) {
                        // найден блок
                            //oidBrecord = btCurrSpace[MSExcel.s_dictEntity[key].m_BlockName];
                            btrCurrent = trCurrent.GetObject(btCurrSpace[blockName]
                                , OpenMode.ForWrite) as BlockTableRecord;
                        } else {
                        // создаем новое определение блока, задаем ему имя
                            btrCurrent = new BlockTableRecord();
                            btrCurrent.Name = blockName;

                            // добавляем созданное определение блока в таблицу блоков, сохраняем его ID
                            //oidBrecord =
                                btCurrSpace.Add(btrCurrent);
                            // добавляем созданное определение блока в транзакцию
                            trCurrent.AddNewlyCreatedDBObject(btrCurrent, true);
                        }
                    } else
                        btrCurrent = trCurrent.GetObject(dbCurrent.CurrentSpaceId
                            , OpenMode.ForWrite) as BlockTableRecord;

                    foreach (KEY_ENTITY key in MSExcel.GetBlock(blockName).Keys) {
                        if (MSExcel.GetEntityCtor(blockName, key).m_entity.Id.IsValid == false) {
                            oidEntity = btrCurrent.AppendEntity(MSExcel.GetEntityCtor(blockName, key).m_entity);
                            trCurrent.AddNewlyCreatedDBObject(MSExcel.GetEntityCtor(blockName, key).m_entity as DBObject, true);

                            if (!(MSExcel.GetEntityCtor(blockName, key).m_ptDisplacement == Point3d.Origin))
                                MSExcel.GetEntityCtor(blockName, key).m_entity.TransformBy(
                                    Matrix3d.Displacement(MSExcel.GetEntityCtor(blockName, key).m_ptDisplacement - Point3d.Origin)
                                );
                            else
                                ;
                        } else
                            ; // уже добавлен
                    }

                    trCurrent.Commit();
                }

                Logging.AcEditorWriteMessage(message);
            } catch (System.Exception e) {
                Logging.AcEditorWriteException(e, message);

                Logging.ExceptionCaller(MethodBase.GetCurrentMethod(), e);
            }
        }

        /// <summary>
        /// Добавить примитив из коллекции
        /// </summary>
        /// <param name="blockName">Наименования блока - владельца прмитива</param>
        /// <param name="key">Ключ примитива из коллекции</param>
        protected void entityAdd(string blockName, KEY_ENTITY key)
        {
            Database dbCurrent = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
            BlockTable btCurrSpace;
            BlockTableRecord btrCurrent;
            ObjectId oidBrecord
                , oidEntity;
            // для трансформации в исходное положение и извлесения значений параметров при создании примитива
            //Entity entityTransformCopy;

            string message = string.Format(@"Добавление примитива {0}, ИД={1}, имя={2}..."
                , MSExcel.GetEntityCtor(blockName, key).GetType().Name
                , key.Id
                , key.Name);

            try {
                using (Transaction trCurrent = dbCurrent.TransactionManager.StartTransaction()) {
                    // открываем таблицу блоков на запись
                    btCurrSpace = trCurrent.GetObject(dbCurrent.BlockTableId, OpenMode.ForWrite) as BlockTable;

                    if (blockName.Equals(string.Empty) == false) {
                    // найти блок; при отсутствии создать
                        if (btCurrSpace.Has(blockName) == true) {
                        // найден блок
                            //oidBrecord = btCurrSpace[MSExcel.s_dictEntity[key].m_BlockName];
                            btrCurrent = trCurrent.GetObject(btCurrSpace[blockName]
                                , OpenMode.ForWrite) as BlockTableRecord;
                        } else {
                        // создаем новое определение блока, задаем ему имя
                            btrCurrent = new BlockTableRecord();
                            btrCurrent.Name = blockName;

                            // добавляем созданное определение блока в таблицу блоков, сохраняем его ID
                            //oidBrecord =
                                btCurrSpace.Add(btrCurrent);
                            // добавляем созданное определение блока в транзакцию
                            trCurrent.AddNewlyCreatedDBObject(btrCurrent, true);
                        }
                    } else
                        btrCurrent = trCurrent.GetObject(dbCurrent.CurrentSpaceId
                            , OpenMode.ForWrite) as BlockTableRecord;

                    oidEntity = btrCurrent.AppendEntity(MSExcel.GetEntityCtor(blockName, key).m_entity);
                    trCurrent.AddNewlyCreatedDBObject(MSExcel.GetEntityCtor(blockName, key).m_entity as DBObject, true);

                    if (!(MSExcel.GetEntityCtor(blockName, key).m_ptDisplacement == Point3d.Origin))
                        MSExcel.GetEntityCtor(blockName, key).m_entity.TransformBy(
                            Matrix3d.Displacement(MSExcel.GetEntityCtor(blockName, key).m_ptDisplacement - Point3d.Origin)
                        );
                    else
                        ;

                    #region для трансформации в исходное положение и извлесения значений параметров при создании примитива
                    //if (MSExcel.s_dictEntity[key].m_entity is Solid3d)
                    //    acdictExt = ((Acad3DSolid)MSExcel.s_dictEntity[key].m_entity.AcadObject).GetExtensionDictionary();
                    //else
                    //    ;

                    //MSExcel.s_dictBlock[blockName].m_dictEntity[key].m_entity.TransformBy(Matrix3d.Rotation(1.57F, new Vector3d(0, 1, 0), Point3d.Origin));

                    //entityTransformCopy = MSExcel.s_dictBlock[blockName].m_dictEntity[key].m_entity.GetTransformedCopy(Matrix3d.PlaneToWorld(new Vector3d(0, 0, 1)));
                    #endregion

                    trCurrent.Commit();
                }

                Logging.AcEditorWriteMessage(message);
            } catch (System.Exception e) {
                Logging.AcEditorWriteException(e, message);

                Logging.ExceptionCaller(MethodBase.GetCurrentMethod(), e);
            }
        }

        protected void referencesBlockAdd(string blockName)
        {
            Database dbCurrent = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
            BlockTable btCurrSpace;
            BlockTableRecord ms
                , btrCurrent;
            BlockReference br;
            ObjectId oidBrecord
                , oidEntity;
            // для трансформации в исходное положение и извлесения значений параметров при создании примитива
            //Entity entityTransformCopy;

            string message = string.Format(@"Добавление ссылок на блок {0}..."
                , blockName);

            try {
                using (Transaction trCurrent = dbCurrent.TransactionManager.StartTransaction()) {
                    // открываем таблицу блоков на запись
                    btCurrSpace = trCurrent.GetObject(dbCurrent.BlockTableId, OpenMode.ForWrite) as BlockTable;
                    // открываем пространство модели на запись
                    ms = (BlockTableRecord)trCurrent.GetObject(btCurrSpace[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    if (btCurrSpace.Has(blockName) == true) {
                        btrCurrent = trCurrent.GetObject(btCurrSpace[blockName]
                            , OpenMode.ForWrite) as BlockTableRecord;

                        foreach (Settings.MSExcel.POINT3D place in Settings.MSExcel.s_dictBlock[blockName].m_ListReference) {
                            // создаем новое вхождение блока, используя ранее сохраненный ID определения блока
                            br = new BlockReference(new Point3d(place.Values), btrCurrent.Id);
                            // добавляем созданное вхождение блока на пространство модели и в транзакцию
                            ms.AppendEntity(br);
                            trCurrent.AddNewlyCreatedDBObject(br, true);
                        }
                    } else
                    // ошибка - ранее добавленный блок не обнаружен
                        ;

                    trCurrent.Commit();
                }
            } catch (System.Exception e) {
                Logging.AcEditorWriteException(e, message);

                Logging.ExceptionCaller(MethodBase.GetCurrentMethod(), e);
            }
        }

        protected void referenceBlockAdd(string blockName, Settings.MSExcel.POINT3D place)
        {
            Database dbCurrent = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
            BlockTable btCurrSpace;
            BlockTableRecord btrCurrent;
            BlockReference br;
            ObjectId oidBrecord
                , oidEntity;
            // для трансформации в исходное положение и извлесения значений параметров при создании примитива
            //Entity entityTransformCopy;

            string message = string.Format(@"Добавление ссылки на блок {0}, размещение {1}..."
                , blockName
                , place.ToString());

            try {
                using (Transaction trCurrent = dbCurrent.TransactionManager.StartTransaction()) {
                    // открываем таблицу блоков на запись
                    btCurrSpace = trCurrent.GetObject(dbCurrent.BlockTableId, OpenMode.ForWrite) as BlockTable;
                    // открываем пространство модели на запись
                    btrCurrent = (BlockTableRecord)trCurrent.GetObject(btCurrSpace[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                    // создаем новое вхождение блока, используя ранее сохраненный ID определения блока
                    br = new BlockReference(Point3d.Origin, btrCurrent.Id);
                    // добавляем созданное вхождение блока на пространство модели и в транзакцию
                    btrCurrent.AppendEntity(br);
                    trCurrent.AddNewlyCreatedDBObject(br, true);

                    trCurrent.Commit();
                }
            } catch (System.Exception e) {
                Logging.AcEditorWriteException(e, message);

                Logging.ExceptionCaller(MethodBase.GetCurrentMethod(), e);
            }
        }

        protected void flash()
        {
            if (MSExcel.s_dictBlock.Count > 0) {
                foreach (string blockName in MSExcel.s_dictBlock.Keys) {
                    // добавить определения (definition) блоков
                    //foreach (KEY_ENTITY key in MSExcel.s_dictBlock[blockName].m_dictEntity.Keys)
                    //    entityAdd(blockName, key);
                    entitiesAdd(blockName);

                    // добавить вхождения/ссылки (reference) блоков 
                    //foreach (MSExcel.BLOCK.PLACEMENT place in MSExcel.s_dictBlock[blockName].m_ListReference)
                    //    referenceBlockAdd(blockName, place);
                    referencesBlockAdd(blockName);
                }
            } else {
                Logging.AcEditorWriteMessage(@"Команда ИМПОРТ не выполнена либо нет объектов...");
            }
        }
        #endregion
    }
}

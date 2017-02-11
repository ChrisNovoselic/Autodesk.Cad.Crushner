﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Autodesk.Cad.Crushner.Settings
{
    partial class EntityParser
    {
        private enum COORD3d { X, Y, Z, COUNT }

        /// <summary>
        /// Создать новый примитив - линия по значениям параметров из строки таблицы
        /// </summary>
        /// <param name="rEntity">Строка таблицы со значениями параметров примитива</param>
        /// <param name="format">Формат файла конфигурации из которого была импортирована таблица</param>
        /// <param name="blockName">Наимнование блока (только при формате 'HEAP')</param>
        /// <returns>Объект примитива - линия</returns>
        public static ProxyEntity newLine(DataRow rEntity, MSExcel.FORMAT format/*, string blockName*/)
        {
            ProxyEntity pEntityRes;

            double[] ptStart = new double[(int)COORD3d.COUNT]
                , ptEnd = new double[(int)COORD3d.COUNT];

            string name = string.Empty;
            MSExcel.COMMAND_ENTITY command = MSExcel.COMMAND_ENTITY.UNKNOWN;

            if (TryParseCommandAndNameEntity(format, rEntity, out name, out command) == true)
                // значения для параметров примитива
                switch (format) {
                    case MSExcel.FORMAT.HEAP:
                        ptStart[(int)COORD3d.X] = double.Parse(rEntity[(int)MSExcel.HEAP_INDEX_COLUMN.LINE_START_X].ToString()
                            , System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                        ptStart[(int)COORD3d.Y] = double.Parse(rEntity[(int)MSExcel.HEAP_INDEX_COLUMN.LINE_START_Y].ToString()
                            , System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                        ptStart[(int)COORD3d.Z] = double.Parse(rEntity[(int)MSExcel.HEAP_INDEX_COLUMN.LINE_START_Z].ToString()
                            , System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);

                        ptEnd[(int)COORD3d.X] = double.Parse(rEntity[(int)MSExcel.HEAP_INDEX_COLUMN.LINE_END_X].ToString()
                            , System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                        ptEnd[(int)COORD3d.Y] = double.Parse(rEntity[(int)MSExcel.HEAP_INDEX_COLUMN.LINE_END_Y].ToString()
                            , System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                        ptEnd[(int)COORD3d.Z] = double.Parse(rEntity[(int)MSExcel.HEAP_INDEX_COLUMN.LINE_END_Z].ToString()
                            , System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);

                        pEntityRes = new ProxyEntity(
                            name
                            , command
                            , new ProxyEntity.Property[] {
                                new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_START_X, ptStart[(int)COORD3d.X])
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_START_Y, ptStart[(int)COORD3d.Y])
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_START_Z, ptStart[(int)COORD3d.Z])
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_END_X, ptEnd[(int)COORD3d.X])
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_END_Y, ptEnd[(int)COORD3d.Y])
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_END_Z, ptEnd[(int)COORD3d.Z])
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_COLORINDEX
                                    , int.Parse(rEntity[(int)MSExcel.HEAP_INDEX_COLUMN.LINE_COLORINDEX].ToString()
                                        , System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture))
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_TICKNESS
                                    , double.Parse(rEntity[(int)MSExcel.HEAP_INDEX_COLUMN.LINE_TICKNESS].ToString()
                                        , System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture))
                            }
                        );

                        //pEntityRes.m_BlockName = blockName;
                        break;
                    case MSExcel.FORMAT.ORDER:
                        ptStart[(int)COORD3d.X] = double.Parse(rEntity[@"START.X"].ToString()
                            , System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                        ptStart[(int)COORD3d.Y] = double.Parse(rEntity[@"START.Y"].ToString()
                            , System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                        ptStart[(int)COORD3d.Z] = double.Parse(rEntity[@"START.Z"].ToString()
                            , System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);

                        ptEnd[(int)COORD3d.X] = double.Parse(rEntity[@"END.X"].ToString()
                            , System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                        ptEnd[(int)COORD3d.Y] = double.Parse(rEntity[@"END.Y"].ToString()
                            , System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                        ptEnd[(int)COORD3d.Z] = double.Parse(rEntity[@"END.Z"].ToString()
                            , System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);

                        pEntityRes = new ProxyEntity(
                            name
                            , command
                            , new ProxyEntity.Property[] {
                                new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_START_X, ptStart[(int)COORD3d.X])
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_START_Y, ptStart[(int)COORD3d.Y])
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_START_Z, ptStart[(int)COORD3d.Z])
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_END_X, ptEnd[(int)COORD3d.X])
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_END_Y, ptEnd[(int)COORD3d.Y])
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_END_Z, ptEnd[(int)COORD3d.Z])
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_COLORINDEX
                                    , int.Parse(rEntity[@"ColorIndex"].ToString()
                                        , System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture))
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_TICKNESS
                                    , double.Parse(rEntity[@"TICKNESS"].ToString()
                                        , System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture))
                            }
                        );

                        break;
                    default:
                        // соэдать примитив по умолчанию
                        pEntityRes = new ProxyEntity();
                        break;
                }
            else {
                pEntityRes = new ProxyEntity();

                Core.Logging.DebugCaller(MethodBase.GetCurrentMethod(), string.Format(@"Ошибка опрделения имени, типа  сущности..."));
            }

            return pEntityRes;
        }
        /// <summary>
        /// Создать новый примитив - линия по значениям параметров из строки таблицы
        /// </summary>
        /// <param name="rEntity">Строка таблицы со значениями параметров примитива</param>
        /// <param name="format">Формат файла конфигурации из которого была импортирована таблица</param>
        /// <param name="blockName">Наимнование блока (только при формате 'HEAP')</param>
        /// <returns>Объект примитива - линия</returns>
        public static EntityParser.ProxyEntity newALineX(DataRow rEntity, MSExcel.FORMAT format/*, string blockName*/)
        {
            EntityParser.ProxyEntity pEntityRes;

            double[] ptStart = new double[(int)COORD3d.COUNT]
                , ptEnd = new double[(int)COORD3d.COUNT];

            string name = string.Empty;
            MSExcel.COMMAND_ENTITY command = MSExcel.COMMAND_ENTITY.UNKNOWN;

            if (TryParseCommandAndNameEntity(format, rEntity, out name, out command) == true)
                // значения для параметров примитива
                switch (format) {
                    case MSExcel.FORMAT.HEAP:
                        ptStart[(int)COORD3d.X] = double.Parse(rEntity[(int)MSExcel.HEAP_INDEX_COLUMN.ALINE_START_X].ToString()
                            , System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                        ptStart[(int)COORD3d.Y] = double.Parse(rEntity[(int)MSExcel.HEAP_INDEX_COLUMN.ALINE_START_Y].ToString()
                            , System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                        ptStart[(int)COORD3d.Z] = double.Parse(rEntity[(int)MSExcel.HEAP_INDEX_COLUMN.ALINE_START_Z].ToString()
                            , System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);

                        ptEnd[(int)COORD3d.X] = ptStart[(int)COORD3d.X] +
                            double.Parse(rEntity[(int)Settings.MSExcel.HEAP_INDEX_COLUMN.ALINE_LENGTH].ToString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                        ptEnd[(int)COORD3d.Y] = ptStart[(int)COORD3d.Y];
                        ptEnd[(int)COORD3d.Z] = ptStart[(int)COORD3d.Z];

                        pEntityRes = new ProxyEntity(
                            name
                            , MSExcel.COMMAND_ENTITY.LINE
                            , new ProxyEntity.Property[] {
                                new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_START_X, ptStart[(int)COORD3d.X])
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_START_Y, ptStart[(int)COORD3d.Y])
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_START_Z, ptStart[(int)COORD3d.Z])
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_END_X, ptEnd[(int)COORD3d.X])
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_END_Y, ptEnd[(int)COORD3d.Y])
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_END_Z, ptEnd[(int)COORD3d.Z])
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_COLORINDEX
                                    , int.Parse(rEntity[(int)MSExcel.HEAP_INDEX_COLUMN.ALINE_COLORINDEX].ToString()
                                        , System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture))
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_TICKNESS
                                    , double.Parse(rEntity[(int)MSExcel.HEAP_INDEX_COLUMN.ALINE_TICKNESS].ToString()
                                        , System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture))
                            }
                        );
                        break;
                    default:
                        // соэдать примитив по умолчанию
                        pEntityRes = new ProxyEntity();
                        break;
                }
            else {
                pEntityRes = new ProxyEntity();

                Core.Logging.DebugCaller(MethodBase.GetCurrentMethod(), string.Format(@"Ошибка опрделения имени, типа  сущности..."));
            }

            return pEntityRes;
        }
        /// <summary>
        /// Создать новый примитив - линия по значениям параметров из строки таблицы
        /// </summary>
        /// <param name="rEntity">Строка таблицы со значениями параметров примитива</param>
        /// <param name="format">Формат файла конфигурации из которого была импортирована таблица</param>
        /// <param name="blockName">Наимнование блока (только при формате 'HEAP')</param>
        /// <returns>Объект примитива - линия</returns>
        public static EntityParser.ProxyEntity newALineY(DataRow rEntity, MSExcel.FORMAT format/*, string blockName*/)
        {
            EntityParser.ProxyEntity pEntityRes;

            double[] ptStart = new double[(int)COORD3d.COUNT]
                , ptEnd = new double[(int)COORD3d.COUNT];

            string name = string.Empty;
            MSExcel.COMMAND_ENTITY command = MSExcel.COMMAND_ENTITY.UNKNOWN;

            if (TryParseCommandAndNameEntity(format, rEntity, out name, out command) == true)
                // значения для параметров примитива
                switch (format) {
                    case MSExcel.FORMAT.HEAP:
                        ptStart[(int)COORD3d.X] = double.Parse(rEntity[(int)MSExcel.HEAP_INDEX_COLUMN.ALINE_START_X].ToString()
                            , System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                        ptStart[(int)COORD3d.Y] = double.Parse(rEntity[(int)MSExcel.HEAP_INDEX_COLUMN.ALINE_START_Y].ToString()
                            , System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                        ptStart[(int)COORD3d.Z] = double.Parse(rEntity[(int)MSExcel.HEAP_INDEX_COLUMN.ALINE_START_Z].ToString()
                            , System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);

                        ptEnd[(int)COORD3d.X] = ptStart[(int)COORD3d.X];
                        ptEnd[(int)COORD3d.Y] = ptStart[(int)COORD3d.Y] +
                            double.Parse(rEntity[(int)Settings.MSExcel.HEAP_INDEX_COLUMN.ALINE_LENGTH].ToString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                        ptEnd[(int)COORD3d.Z] = ptStart[(int)COORD3d.Z];

                        pEntityRes = new ProxyEntity(
                            name
                            , MSExcel.COMMAND_ENTITY.LINE
                            , new ProxyEntity.Property[] {
                                new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_START_X, ptStart[(int)COORD3d.X])
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_START_Y, ptStart[(int)COORD3d.Y])
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_START_Z, ptStart[(int)COORD3d.Z])
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_END_X, ptEnd[(int)COORD3d.X])
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_END_Y, ptEnd[(int)COORD3d.Y])
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_END_Z, ptEnd[(int)COORD3d.Z])
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_COLORINDEX
                                    , int.Parse(rEntity[(int)MSExcel.HEAP_INDEX_COLUMN.ALINE_COLORINDEX].ToString()
                                        , System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture))
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_TICKNESS
                                    , double.Parse(rEntity[(int)MSExcel.HEAP_INDEX_COLUMN.ALINE_TICKNESS].ToString()
                                        , System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture))
                            }
                        );
                        break;
                    default:
                        // соэдать примитив по умолчанию
                        pEntityRes = new ProxyEntity();
                        break;
                }
            else {
                pEntityRes = new ProxyEntity();

                Core.Logging.DebugCaller(MethodBase.GetCurrentMethod(), string.Format(@"Ошибка опрделения имени, типа  сущности..."));
            }

            return pEntityRes;
        }
        /// <summary>
        /// Создать новый примитив - линия по значениям параметров из строки таблицы
        /// </summary>
        /// <param name="rEntity">Строка таблицы со значениями параметров примитива</param>
        /// <param name="format">Формат файла конфигурации из которого была импортирована таблица</param>
        /// <param name="blockName">Наимнование блока (только при формате 'HEAP')</param>
        /// <returns>Объект примитива - линия</returns>
        public static EntityParser.ProxyEntity newALineZ(DataRow rEntity, MSExcel.FORMAT format/*, string blockName*/)
        {
            EntityParser.ProxyEntity pEntityRes;

            double[] ptStart = new double[(int)COORD3d.COUNT]
                , ptEnd = new double[(int)COORD3d.COUNT];

            string name = string.Empty;
            MSExcel.COMMAND_ENTITY command = MSExcel.COMMAND_ENTITY.UNKNOWN;

            if (TryParseCommandAndNameEntity(format, rEntity, out name, out command) == true)
                // значения для параметров примитива
                switch (format) {
                    case MSExcel.FORMAT.HEAP:
                        ptStart[(int)COORD3d.X] = double.Parse(rEntity[(int)MSExcel.HEAP_INDEX_COLUMN.ALINE_START_X].ToString()
                            , System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                        ptStart[(int)COORD3d.Y] = double.Parse(rEntity[(int)MSExcel.HEAP_INDEX_COLUMN.ALINE_START_Y].ToString()
                            , System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                        ptStart[(int)COORD3d.Z] = double.Parse(rEntity[(int)MSExcel.HEAP_INDEX_COLUMN.ALINE_START_Z].ToString()
                            , System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);

                        ptEnd[(int)COORD3d.X] = ptStart[(int)COORD3d.X];
                        ptEnd[(int)COORD3d.Y] = ptStart[(int)COORD3d.Y];
                        ptEnd[(int)COORD3d.Z] = ptStart[(int)COORD3d.Z] +
                            double.Parse(rEntity[(int)Settings.MSExcel.HEAP_INDEX_COLUMN.ALINE_LENGTH].ToString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);

                        pEntityRes = new ProxyEntity(
                            name
                            , MSExcel.COMMAND_ENTITY.LINE
                            , new ProxyEntity.Property[] {
                                new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_START_X, ptStart[(int)COORD3d.X])
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_START_Y, ptStart[(int)COORD3d.Y])
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_START_Z, ptStart[(int)COORD3d.Z])
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_END_X, ptEnd[(int)COORD3d.X])
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_END_Y, ptEnd[(int)COORD3d.Y])
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_END_Z, ptEnd[(int)COORD3d.Z])
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_COLORINDEX
                                    , int.Parse(rEntity[(int)MSExcel.HEAP_INDEX_COLUMN.ALINE_COLORINDEX].ToString()
                                        , System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture))
                                , new ProxyEntity.Property ((int)MSExcel.HEAP_INDEX_COLUMN.LINE_TICKNESS
                                    , double.Parse(rEntity[(int)MSExcel.HEAP_INDEX_COLUMN.ALINE_TICKNESS].ToString()
                                        , System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture))
                            }
                        );
                        break;
                    default:
                        // соэдать примитив по умолчанию
                        pEntityRes = new ProxyEntity();
                        break;
                }
            else {
                pEntityRes = new ProxyEntity();

                Core.Logging.DebugCaller(MethodBase.GetCurrentMethod(), string.Format(@"Ошибка опрделения имени, типа  сущности..."));
            }

            return pEntityRes;
        }
        /// <summary>
        /// Создать новый примитив - линия по значениям параметров из строки таблицы
        /// </summary>
        /// <param name="rEntity">Строка таблицы со значениями параметров примитива</param>
        /// <param name="format">Формат файла конфигурации из которого была импортирована таблица</param>
        /// <param name="blockName">Наимнование блока (только при формате 'HEAP')</param>
        /// <returns>Объект примитива - линия</returns>
        public static EntityParser.ProxyEntity newRLineX(DataRow rEntity, MSExcel.FORMAT format/*, string blockName*/)
        {
            EntityParser.ProxyEntity pEntityRes;

            double[] ptStart = new double[(int)COORD3d.COUNT]
                , ptEnd = new double[(int)COORD3d.COUNT];
            string nameEntityRelative = string.Empty;

            // значения для параметров примитива
            switch (format) {
                case MSExcel.FORMAT.HEAP:
                    nameEntityRelative = rEntity[(int)Settings.MSExcel.HEAP_INDEX_COLUMN.RLINE_NAME_ENTITY_RELATIVE].ToString();

                    ptEnd[(int)COORD3d.X] =
                        double.Parse(rEntity[(int)Settings.MSExcel.HEAP_INDEX_COLUMN.RLINE_LENGTH].ToString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                    //???
                    pEntityRes = new ProxyEntity();
                    break;
                default:
                    // соэдать примитив по умолчанию
                    pEntityRes = new ProxyEntity();
                    break;
            }

            return pEntityRes;
        }

        public static EntityParser.ProxyEntity newRLineY(DataRow rEntity, MSExcel.FORMAT format/*, string blockName*/)
        {
            EntityParser.ProxyEntity pEntityRes;

            double[] ptStart = new double[(int)COORD3d.COUNT]
                , ptEnd = new double[(int)COORD3d.COUNT];
            string nameEntityRelative = string.Empty;

            // значения для параметров примитива
            switch (format) {
                case MSExcel.FORMAT.HEAP:
                    nameEntityRelative = rEntity[(int)Settings.MSExcel.HEAP_INDEX_COLUMN.RLINE_NAME_ENTITY_RELATIVE].ToString();

                    ptEnd[(int)COORD3d.Y] =
                        double.Parse(rEntity[(int)Settings.MSExcel.HEAP_INDEX_COLUMN.RLINE_LENGTH].ToString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                    //???
                    pEntityRes = new ProxyEntity();
                    break;
                default:
                    // соэдать примитив по умолчанию
                    pEntityRes = new ProxyEntity();
                    break;
            }

            return pEntityRes;
        }

        public static EntityParser.ProxyEntity newRLineZ(DataRow rEntity, MSExcel.FORMAT format/*, string blockName*/)
        {
            EntityParser.ProxyEntity pEntityRes;

            double[] ptStart = new double[(int)COORD3d.COUNT]
                , ptEnd = new double[(int)COORD3d.COUNT];
            string nameEntityRelative = string.Empty;

            // значения для параметров примитива
            switch (format) {
                case MSExcel.FORMAT.HEAP:
                    nameEntityRelative = rEntity[(int)Settings.MSExcel.HEAP_INDEX_COLUMN.RLINE_NAME_ENTITY_RELATIVE].ToString();

                    ptEnd[(int)COORD3d.Z] =
                        double.Parse(rEntity[(int)Settings.MSExcel.HEAP_INDEX_COLUMN.RLINE_LENGTH].ToString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                    //???
                    pEntityRes = new ProxyEntity();
                    break;
                default:
                    // соэдать примитив по умолчанию
                    pEntityRes = new ProxyEntity();
                    break;
            }

            return pEntityRes;
        }
    }
}
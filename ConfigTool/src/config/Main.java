package config;

import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;

import org.apache.poi.ss.usermodel.Cell;
import org.apache.poi.ss.usermodel.FormulaEvaluator;
import org.apache.poi.ss.usermodel.Row;
import org.apache.poi.ss.usermodel.Sheet;
import org.apache.poi.ss.usermodel.Workbook;
import org.apache.poi.ss.usermodel.WorkbookFactory;

import com.fasterxml.jackson.databind.ObjectMapper;

public class Main {
	public static final String SEPARATOR = ",";

	public static void main(String[] args) {
		if (args.length > 0 && args.length != 3) {
			System.out.println("Usage: configtool.jar [default/battlequest/constant] input.xls output.json");
			return;
		}

		if (args.length == 3) {
			String mode = args[0];
			if ("default".equals(mode)) {
				processDefault(args[1], args[2]);
			} else if ("battlequest".equals(mode)) {
				processBattleQuest(args[1], args[2]);
			} else if ("constant".equals(mode)) {
				processConstant(args[1], args[2]);
			}
		} else {
			// Convert all excel files in running folder
			File currentDir = new File(System.getProperty("user.dir"));
			File[] files = currentDir.listFiles();
			for (int i = 0; i < files.length; i++) {
				String filename = files[i].getName();
				if (filename.endsWith(".xlsx")) {
					String output = filename.substring(0, filename.length() - 5) + ".txt";
					if (filename.toLowerCase().indexOf("constant") != -1) {
						processConstant(filename, output);
					} else if (filename.indexOf("battlequest") != -1) {
						processBattleQuest(filename, output);
					} else {
						// if(filename.equals("allSkill.xlsx") ||
						// filename.equals("allStat.xlsx"))
						processDefault(filename, output);
					}
				}
			}
		}
	}

	private static void processBattleQuest(String input, String output) {
		FileInputStream fis = null;
		String sheetName = null;
		int row = -1;
		int col = -1;
		try {
			HashMap<String, Object> configData = new HashMap<String, Object>();
			fis = new FileInputStream(input);
			Workbook wb = WorkbookFactory.create(fis);
			Sheet data = wb.getSheet("data");
			if (data != null) {
				HashMap<Integer, String> battleData = new HashMap<Integer, String>();
				for (int i = 0; i < 6; i++) {
					Row r = data.getRow(i);
					for (int j = 0; j < r.getLastCellNum(); j++) {
						Cell c = r.getCell(j);
						String cellValue = c.getStringCellValue();

						if ("X".equalsIgnoreCase(cellValue.trim()))
							break;

						if (!"-".equals(cellValue.trim())) {
							battleData.put(j * 6 + i, cellValue);
							if (cellValue.startsWith("HERO-")) {
								// Get hero config
								if (configData.get(cellValue) == null) {
									Sheet heroSheet = wb.getSheet(cellValue);
									if (heroSheet == null) {
										throw new Exception("Cannot find hero data: " + cellValue);
									}
									HashMap<String, Object> heroData = new HashMap<String, Object>();
									heroData.put("type", heroSheet.getRow(0).getCell(1).getNumericCellValue());
									heroData.put("level", heroSheet.getRow(1).getCell(1).getNumericCellValue());

									ArrayList<String> itemList = new ArrayList<String>();
									Row itemRow = heroSheet.getRow(2);
									Iterator<Cell> itemIter = itemRow.cellIterator();
									while (itemIter.hasNext()) {
										Cell itemCell = itemIter.next();
										if (itemCell.getColumnIndex() == 0)
											continue;
										String itemStr = itemCell.getStringCellValue().trim();
										if (!itemStr.equals(""))
											itemList.add(itemStr);
									}
									heroData.put("items", itemList);

									ArrayList<String> skillList = new ArrayList<String>();
									Row skillRow = heroSheet.getRow(3);
									Iterator<Cell> skillIter = skillRow.cellIterator();
									while (skillIter.hasNext()) {
										Cell skillCell = skillIter.next();
										if (skillCell.getColumnIndex() == 0)
											continue;
										String skillStr = skillCell.getStringCellValue().trim();
										if (!skillStr.equals(""))
											skillList.add(skillStr);
									}
									heroData.put("skills", skillList);

									configData.put(cellValue, heroData);
								}
							}
						}
					}
				}
				configData.put("data", battleData);

				ObjectMapper mapper = new ObjectMapper();
				mapper.writeValue(new File(output), configData);
			} else {
				System.out.println("Cannot find data sheet!");
			}
		} catch (Exception e) {
			e.printStackTrace();
			System.out.println(input + "___" + sheetName + "___" + row + "___" + col);
		} finally {
			if (fis != null) {
				try {
					fis.close();
				} catch (IOException e) {
					e.printStackTrace();
				}
			}
		}
	}

	private static void processConstant(String input, String output) {
		FileInputStream fis = null;
		String sheetName = null;
		int row = -1;
		int col = -1;
		try {
			// Map sheetId => sheetData
			HashMap<String, Object> configData = new HashMap<String, Object>();
			fis = new FileInputStream(input);
			Workbook wb = WorkbookFactory.create(fis);
			int sheetCount = wb.getNumberOfSheets();
			for (int sid = 0; sid < sheetCount; sid++) {
				Sheet sheet = wb.getSheetAt(sid);
				sheetName = sheet.getSheetName();
				String sIdx = getID(sheetName);
				if (sIdx != null) {
					// Map key => value
					HashMap<String, Object> sheetData = new HashMap<String, Object>();
					Iterator<Row> rit = sheet.rowIterator();
					while (rit.hasNext()) {
						Row r = rit.next();
						row = r.getRowNum();
						if (r.getLastCellNum() < 2)
							continue;
						// Get row key
						String key = r.getCell(0).getStringCellValue();
						Cell valueCell = r.getCell(1);
						sheetData.put(key, getCellValue(valueCell));
					}
					configData.put(sIdx, sheetData);
				}
			}

			ObjectMapper mapper = new ObjectMapper();
			mapper.writeValue(new File(output), configData);
		} catch (Exception e) {
			e.printStackTrace();
			System.out.println(input + "___" + sheetName + "___" + row + "___" + col);
		} finally {
			if (fis != null) {
				try {
					fis.close();
				} catch (IOException e) {
					e.printStackTrace();
				}
			}
		}
	}

	@SuppressWarnings("unchecked")
	private static void processDefault(String input, String output) {
		FileInputStream fis = null;
		String sheetName = null;
		int row = -1;
		int col = -1;
		try {
			// Map sheetId => sheetData
			HashMap<String, Object> configData = new HashMap<String, Object>();
			fis = new FileInputStream(input);
			Workbook wb = WorkbookFactory.create(fis);
			FormulaEvaluator evaluator = wb.getCreationHelper().createFormulaEvaluator();
			int sheetCount = wb.getNumberOfSheets();
			for (int sid = 0; sid < sheetCount; sid++) {
				Sheet sheet = wb.getSheetAt(sid);
				sheetName = sheet.getSheetName();
				String sIdx = getID(sheetName);
				if (sIdx != null) {
					HashMap<String, Object> sheetData = new HashMap<String, Object>();
					int columnCount = 0;
					int idDepth = 0;
					int headerDepth = 0;
					String[] idList = null;
					ArrayList<String[]> headers = new ArrayList<String[]>();

					Iterator<Row> rit = sheet.rowIterator();
					while (rit.hasNext()) {
						Row r = rit.next();
						row = r.getRowNum();

						if (columnCount == 0)
							columnCount = r.getLastCellNum();
						if (r.getLastCellNum() < columnCount)
							continue;

						// Check if it is a header row
						if (headerDepth == 0 && (headers.size() == 0 || "".equals(r.getCell(0).toString().trim()))) {
							// Read headers data
							String[] rowHeader = new String[columnCount];
							for (int i = 0; i < rowHeader.length; i++) {
								col = i;
								if(r.getCell(i) == null)
								{
									columnCount--;
									continue;
								}
								rowHeader[i] = getID(r.getCell(i).toString());
							}
							headers.add(rowHeader);

							if (idDepth == 0) {
								for (int i = 0; i < rowHeader.length; i++) {
									if (rowHeader[i] == null)
										idDepth++;
									else
										break;
								}
								idList = new String[idDepth];
							}
						} else {
							headerDepth = headers.size();

							// Get row id
							for (int i = 0; i < idDepth; i++) {
								Cell c = r.getCell(i);
								String rIdx = null;
								int type = c.getCellType();
								if (type == Cell.CELL_TYPE_NUMERIC)
									rIdx = String.valueOf((int) (c.getNumericCellValue()));
								else if (type == Cell.CELL_TYPE_STRING)
									rIdx = c.getStringCellValue();
								if (rIdx != null) {
									idList[i] = rIdx;
								}
							}

							// Get row data map
							HashMap<String, Object> rowData = sheetData;
							for (int i = 0; i < idDepth; i++) {
								HashMap<String, Object> subData = (HashMap<String, Object>) rowData.get(idList[i]);
								if (subData == null) {
									subData = new HashMap<String, Object>();
									rowData.put(idList[i], subData);
								}
								rowData = subData;
							}

							for (int i = idDepth; i < columnCount; i++) {
								col = i;

								// Check if it's a column without header
								boolean isNullColumn = true;
								for (int j = 0; j < headerDepth; j++) {
									if (headers.get(j)[i] != null) {
										isNullColumn = false;
										break;
									}
								}
								if (isNullColumn)
									continue;

								HashMap<String, Object> headerData = rowData;
								// Check if it's a single header column
								if (headerDepth == 1 || (headers.get(0)[i] != null && headers.get(1)[i] == null)) {
									Object value = getCellValue(evaluator.evaluateInCell(r.getCell(i)));
									headerData.put(headers.get(0)[i], value);								
								}
								// Column with multiple-level headers
								else {
									// Find highest depth header
									int lastHeaderIdx = -1;
									for (int j = headerDepth - 1; j >= 0; j--) {
										if (headers.get(j)[i] != null) {
											lastHeaderIdx = j;
											break;
										}
									}

									for (int j = 0; j < lastHeaderIdx; j++) {
										String headerName = headers.get(j)[i];
										if (headerName == null) {
											// Find merge header
											for (int xxx = i - 1; xxx >= idDepth; xxx--) {
												if (headers.get(j)[xxx] != null) {
													headerName = headers.get(j)[xxx];
													break;
												}
											}
										}
										if (headerName == null)
											continue;
										HashMap<String, Object> subHeader = (HashMap<String, Object>) headerData.get(headerName);
										if (subHeader == null) {
											subHeader = new HashMap<String, Object>();
											headerData.put(headerName, subHeader);
										}
										headerData = subHeader;
									}
									headerData.put(headers.get(lastHeaderIdx)[i], getCellValue(evaluator.evaluateInCell(r.getCell(i))));
								}
							}
						}
					}
					configData.put(sIdx, sheetData);
				}
			}

			ObjectMapper mapper = new ObjectMapper();
			mapper.writeValue(new File(output), configData);
		} catch (Exception e) {
			e.printStackTrace();
			System.out.println(input + "___" + sheetName + "___" + row + "___" + col);
		} finally {
			if (fis != null) {
				try {
					fis.close();
				} catch (IOException e) {
					e.printStackTrace();
				}
			}
		}
	}

	private static String getID(String input) {
		String result = null;
		try {
			if (input.startsWith("(")) {
				result = input.substring(1, input.indexOf(")"));
			}
		} catch (Exception e) {
		}
		return result;
	}
	
	private static Object getCellValue(Cell cell) {
		int cellType = cell.getCellType();
		switch (cellType) {
		case Cell.CELL_TYPE_NUMERIC:
			double value = cell.getNumericCellValue();
			if(value == (long)value){
				return (long)value;
			}else {
				return value;
			}
		
		case Cell.CELL_TYPE_BOOLEAN:
			return cell.getBooleanCellValue();
		case Cell.CELL_TYPE_STRING:
			String stringCellValue = cell.getStringCellValue();
			if (stringCellValue.indexOf(SEPARATOR) > -1) {
				return processStringData(stringCellValue);
			} else {
				return stringCellValue.trim();
			}
		default:
			return null;
		}
	}
	
	public static boolean isDouble(String str) {
		try {
			Double.parseDouble(str);
			return true;
		}catch(Exception e){
			return false;
		}
	}
	
	public static boolean isLong(String str) {
		try {
			Long.parseLong(str);
			return true;
		}catch(Exception e){
			return false;
		}
	}
	
	public static List<Object> processStringData(String stringCellValue){
		String[] arrValue = stringCellValue.split(SEPARATOR);
		List<Object> ret = new ArrayList<Object>();
		if(isLong(arrValue[0])) {
			for (String str : arrValue) {
				str = str.trim();
				if (!str.equals("")) {
					ret.add(Long.parseLong(str));
				}
			}
			return ret;
		}else if(isDouble(arrValue[0])){
			for (String str : arrValue) {
				str = str.trim();
				if (!str.equals("")) {
					ret.add(Double.parseDouble(str));
				}
			}
			return ret;
		}else {
			for (String str : arrValue) {
				str = str.trim();
				if (!str.equals("")) {
					ret.add(str);
				}
			}
			return ret;
		}
	}

}

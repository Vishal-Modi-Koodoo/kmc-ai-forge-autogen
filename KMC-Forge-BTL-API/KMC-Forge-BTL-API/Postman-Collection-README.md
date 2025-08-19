# KMC Forge BTL API - Postman Collection

This Postman collection provides comprehensive testing for the KMC Forge BTL API, including multiple file upload functionality.

## 📥 How to Import

1. **Open Postman**
2. **Click "Import"** button (top left)
3. **Select the file**: `KMC-Forge-BTL-API.postman_collection.json`
4. **Click "Import"**

## 🚀 Quick Start

### 1. Set Environment Variables
- The collection uses a variable `{{base_url}}` set to `http://localhost:5001`
- You can modify this in the collection variables if your API runs on a different port

### 2. Start the API
```bash
cd KMC-Forge-BTL-API
dotnet run --project KMC-Forge-BTL-API
```

### 3. Test Multiple File Upload

#### Option A: Using Postman Collection
1. Open the collection in Postman
2. Navigate to **"File Upload"** → **"Upload Multiple Files - Portfolio"**
3. In the **Body** tab, select **"form-data"**
4. Add multiple files with key `files`:
   - Click **"Select Files"** for each file field
   - Choose your PDF, image, or Excel files
5. Click **"Send"**

#### Option B: Using Custom Test Page
1. Open: `http://localhost:5001/file-upload-test.html`
2. Select multiple files using the file picker
3. Click **"Upload Files"**

## 📋 Available Endpoints

### File Upload Endpoints
- **POST** `/api/DocumentUpload/upload2` - Upload multiple files for portfolio processing

### Document Identification Endpoints
- **POST** `/api/DocumentIdentification/identify-from-text` - Identify document type from text
- **POST** `/api/DocumentIdentification/identify-from-image` - Identify document type from image
- **GET** `/api/DocumentIdentification/supported-types` - Get supported document types

### Health & Status Endpoints
- **GET** `/health` - Health check
- **GET** `/swagger` - Swagger UI documentation
- **GET** `/file-upload-test.html` - Custom file upload test page

## 📁 Supported File Types

- **PDF files**: `.pdf`
- **Images**: `.jpg`, `.jpeg`, `.png`, `.gif`, `.bmp`
- **Excel files**: `.xlsx`, `.xls`
- **CSV files**: `.csv`
- **Word documents**: `.doc`, `.docx`

## 🔧 Configuration

### File Size Limits
- **Maximum file size**: 50MB per file
- **Maximum request size**: 500MB total
- **Supported formats**: PDF, Images, Excel, CSV, Word

### Response Format
```json
{
  "portfolioId": "guid-string",
  "estimatedProcessingTime": "3-5 minutes",
  "status": "Processing",
  "uploadedDocuments": [...],
  "invalidDocuments": [...],
  "summary": {
    "totalDocuments": 3,
    "validDocuments": 2,
    "invalidDocuments": 1,
    "processingCompleted": true
  }
}
```

## 🧪 Testing Scenarios

### 1. Single File Upload
- Use **"Upload Single File - Portfolio"** request
- Select one file of any supported type
- Verify response contains portfolio ID and processing status

### 2. Multiple File Upload
- Use **"Upload Multiple Files - Portfolio"** request
- Select 2-3 different file types (PDF + Image + Excel)
- Verify all files are processed and results are returned

### 3. Invalid File Types
- Try uploading unsupported file types (e.g., `.txt`, `.zip`)
- Verify files are marked as invalid with appropriate error messages

### 4. Large Files
- Test with files接近 50MB limit
- Verify file size validation works correctly

## 🐛 Troubleshooting

### Common Issues

1. **Connection Refused**
   - Ensure API is running on `http://localhost:5001`
   - Check if port is available

2. **File Upload Fails**
   - Verify file size is under 50MB
   - Check file type is supported
   - Ensure proper `multipart/form-data` format

3. **Timeout Errors**
   - Large files may take time to process
   - Check API logs for processing status

### Debug Steps

1. **Check API Health**
   - Use **"Health Check"** request
   - Should return 200 OK

2. **Verify File Upload**
   - Use **"File Upload Test Page"** request
   - Access the HTML interface for visual testing

3. **Check Swagger Documentation**
   - Use **"Swagger UI"** request
   - Review API documentation and test endpoints

## 📝 Notes

- The API processes files asynchronously
- Each file is analyzed by AI for document type identification
- Invalid files are returned with detailed error messages
- All valid files are stored locally and processed through the AI pipeline
- Portfolio ID is generated for each upload session

## 🔗 Related Files

- `KMC-Forge-BTL-API.postman_collection.json` - Postman collection
- `wwwroot/file-upload-test.html` - Custom test interface
- `Controllers/DocumentUploadController.cs` - Upload endpoint implementation

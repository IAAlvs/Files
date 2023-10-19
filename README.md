# File Upload API Documentation

## Files

This is a WebApi that allows you to upload files to a cloud storage using .NET 7.

## Running the API
-------------------------------------------------

To run the API, execute the following command in your terminal:

`dotnet run`
## Running the api with docker
-------------------------------------------------
To run the API, with docker execute the following commands --using Unix OS having docker with sudo permissions--

**script to build image**
docker build -t imagename:tag . 
**script to run container**
docker run -e AUTH0_DOMAIN=your_auth0_domain \
           -e AUTH0_AUDIENCE=your_auth0_audience \
           -e AWS_ACCESS_KEY=your_aws_access_key \
           -e AWS_SECRET_KEY=your_aws_secret_key \
           -e AWS_BUCKET_NAME=your_aws_bucket_name \
           -e AWS_BUCKET_REGION=your_aws_bucket_region \
           -e DB_CONNECTION=your_db_connection_string \
           -d imagename:tag


## View api endpoints docs with swagger
-------------------------------------------------


Open your browser in *YOURDOMAIN/api/v1/files/swagger/index.html* only works in *development* environment

## Running Tests
-------------------------------------------------


To run the tests for this API, execute the following command:

`dotnet test`

## Running Update Database Based on Current Migrations
-------------------------------------------------


Before you can update the database based on the current migrations, you need to install the global .NET tool "ef." To update the database, execute the following command:

`dotnet ef database update`

## Public Storage
-------------------------------------------------


Public storage is a Blob storage where public files can be accessed by anyone.

## Private Storage
-------------------------------------------------


Private storage is a Blob storage where public files cannot be accessed directly. However, you can create temporary URLs to access files when you have security concerns, such as CORS to restrict access to specific domains.

For more information and details on how to use this API, please refer to the source code and documentation provided.

## Environment Variables Required to Run the Project
-------------------------------------------------

The following environment variables are required to run the project. Make sure to set these variables with the appropriate values in your environment.

Variable Name

Description

`AUTH0_DOMAIN`

The project uses Auth0 for authentication. You need to set this variable to your Auth0 domain.

`AUTH0_AUDIENCE`

The project uses Auth0 to manage authentication. Set this variable to your Auth0 audience.

`AWS_ACCESS_KEY`

The project uses AWS S3 for storage. Set this variable to your AWS access key.

`AWS_SECRET_KEY`

The project uses AWS S3 for storage. Set this variable to your AWS secret key.

`AWS_BUCKET_NAME`

The project uses AWS S3 for storage. Set this variable to the name of your AWS S3 bucket.

`AWS_BUCKET_REGION`

The project uses AWS S3 for storage. Set this variable to the AWS region where your S3 bucket is located.

`DB_CONNECTION`

The project uses PostgreSQL to manage chunks and files storage. Set this variable to your PostgreSQL connection string. Example:  
`Port=5432;Username=youruserdb;Password=youruserpassword;Database=dbname;Host=databasehostdb`
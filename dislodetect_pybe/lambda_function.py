import json
import base64
from io import BytesIO
import os
from app import app

def lambda_handler(event, context):
    """
    AWS Lambda handler for Flask app
    """
    try:
        # Handle API Gateway proxy integration
        if 'httpMethod' in event:
            # Convert API Gateway event to WSGI environ
            from werkzeug.serving import WSGIRequestHandler
            from werkzeug.wrappers import Request
            
            # Create a simple WSGI environ from the event
            environ = {
                'REQUEST_METHOD': event['httpMethod'],
                'PATH_INFO': event['path'],
                'QUERY_STRING': event.get('queryStringParameters', '') or '',
                'CONTENT_TYPE': event.get('headers', {}).get('content-type', ''),
                'CONTENT_LENGTH': str(len(event.get('body', '') or '')),
                'SERVER_NAME': 'localhost',
                'SERVER_PORT': '80',
                'wsgi.input': BytesIO((event.get('body', '') or '').encode()),
                'wsgi.errors': BytesIO(),
                'wsgi.version': (1, 0),
                'wsgi.multithread': False,
                'wsgi.multiprocess': True,
                'wsgi.run_once': False,
                'wsgi.url_scheme': 'https',
            }
            
            # Add headers to environ
            for key, value in event.get('headers', {}).items():
                key = key.upper().replace('-', '_')
                if key not in ('CONTENT_TYPE', 'CONTENT_LENGTH'):
                    environ[f'HTTP_{key}'] = value
            
            # Handle the request
            response_data = []
            def start_response(status, headers):
                response_data.append(status)
                response_data.append(headers)
            
            app_response = app(environ, start_response)
            response_body = b''.join(app_response).decode()
            
            return {
                'statusCode': int(response_data[0].split()[0]),
                'headers': dict(response_data[1]),
                'body': response_body
            }
        
        # Handle direct invocation (for testing)
        else:
            return {
                'statusCode': 200,
                'body': json.dumps({'message': 'Lambda function is running'})
            }
            
    except Exception as e:
        print(f"Error: {str(e)}")
        return {
            'statusCode': 500,
            'body': json.dumps({'error': str(e)})
        }
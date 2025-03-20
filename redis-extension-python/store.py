import uuid
from redis_connection import get_redis_connection

class Store:
    def __init__(self):
        self.db = get_redis_connection()
    
    def init(self):
        for i in range(100):
            self.db.set(str(i), str(uuid.uuid4()))
    
    def set(self, key, value):
        self.db.set(key, value)
    
    def get(self, key):
        value = self.db.get(key)
        return value if value else None
    
    def get_all(self):
        return [self.db.get(str(i)) for i in range(100)]

import redis
import os

def get_redis_connection():
    return redis.Redis(host=os.getenv("REDIS_HOST", "localhost"), port=6379, decode_responses=True)

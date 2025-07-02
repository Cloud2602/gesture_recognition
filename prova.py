import pyrealsense2 as rs
pipe = rs.pipeline()
cfg = rs.config()
cfg.enable_stream(rs.stream.depth, 640, 480, rs.format.z16, 30)
cfg.enable_stream(rs.stream.color, 1920, 1080, rs.format.bgr8, 30)
profile = pipe.start(cfg)
try:
  for i in range(0, 100):
    frames = pipe.wait_for_frames()
    for f in frames:
      print(f.profile)
finally:
    pipe.stop()
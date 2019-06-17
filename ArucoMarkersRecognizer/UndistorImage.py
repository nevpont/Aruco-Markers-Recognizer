import cherrypy
import numpy as np
import cv2


DIM=(1280, 720)
K=np.array([[357.29938575738356, 0.0, 637.921956136952], [0.0, 357.15390274837864, 370.60510902708484], [0.0, 0.0, 1.0]])
D=np.array([[-0.04444259996484069], [0.002157028902209675], [-0.004983571262210739], [0.0009597873674708474]])


class Root(object):
    @cherrypy.expose
    def undistorte(self):
        data = cherrypy.request.body.read()

        img_data = np.frombuffer(data, np.uint8)
        img = cv2.imdecode(img_data, -1)

        nk = K.copy()
        nk[0, 0] = K[0, 0] / 2
        nk[1, 1] = K[1, 1] / 2
        map1, map2 = cv2.fisheye.initUndistortRectifyMap(K, D, np.eye(3), nk, DIM, cv2.CV_16SC2)
        undistorted_img = cv2.remap(img, map1, map2, interpolation=cv2.INTER_LINEAR, borderMode=cv2.BORDER_CONSTANT)

        return cv2.imencode('.jpg', undistorted_img)[1].tostring()


if __name__ == '__main__':
    cherrypy.quickstart(Root())